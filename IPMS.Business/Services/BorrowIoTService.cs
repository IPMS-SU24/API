﻿using IPMS.Business.Common.Utils;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.DataAccess.Models;
using IPMS.Business.Common.Enums;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using IPMS.Business.Requests.IoTComponent;
using IPMS.Business.Responses.ProjectDashboard;
using IPMS.Business.Common.Exceptions;
using IPMS.DataAccess.Common.Enums;
using System.Security.Claims;

namespace IPMS.Business.Services
{
    public class BorrowIoTService : IBorrowIoTService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonServices _commonServices;
        private readonly IMapper _mapper;
        public BorrowIoTService(IUnitOfWork unitOfWork, ICommonServices commonServices, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _commonServices = commonServices;
            _mapper = mapper;
        }

        public async Task<bool> CheckIoTValid(IoTModelRequest request, Guid leaderId)
        {
            if (request.Quantity <= 0) return false;
            var currentSemester = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester;
            var studiesIn = await _commonServices.GetStudiesIn(leaderId);
            var project = await _commonServices.GetProject(leaderId);
            var @class = await _commonServices.GetCurrentClass(studiesIn.Select(x => x.ClassId), currentSemester.Id);
            //Check borrow Assessment Status => Not In Progress => Validate Fail
            var borrowAssessmentStatus = await _commonServices.GetBorrowIoTStatus(project.Id, @class);
            if (borrowAssessmentStatus != AssessmentStatus.InProgress) return false;
            //Check Exist
            var iot = await _unitOfWork.IoTComponentRepository.GetByIDAsync(request.ComponentId);
            if (iot == null) return false;
            //Check iot in iot list of Topic
            var topicId = await _unitOfWork.ClassTopicRepository.Get().Where(x => x.ProjectId == project.Id).Select(x => x.TopicId).FirstOrDefaultAsync();
            var isInTopicComponent = await _unitOfWork.ComponentsMasterRepository.GetTopicComponents()
                                                                                    .Where(x => x.MasterId == topicId && x.ComponentId == request.ComponentId).AnyAsync();
            if (!isInTopicComponent) return false;
            //Check remain Quantity
            var remainQuantity = await _commonServices.GetRemainComponentQuantityOfLecturer(@class.LecturerId!.Value, request.ComponentId);
            if (remainQuantity < request.Quantity) return false;
            return true;
        }

        public async Task<IEnumerable<BorrowIoTComponentInformation>> GetAvailableIoTComponents(GetAvailableComponentRequest request, Guid leaderId)
        {
            var result = new List<BorrowIoTComponentInformation>();
            var currentSemester = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester;
            var studiesIn = await _commonServices.GetStudiesIn(leaderId);
            var @class = await _commonServices.GetCurrentClass(studiesIn.Select(x => x.ClassId), currentSemester.Id);
            var componentTopics = await _unitOfWork.ComponentsMasterRepository.GetTopicComponents().Where(x => x.MasterId == request.TopicId).Include(x=>x.Component).ToListAsync();
            if (!componentTopics.Any()) throw new DataNotFoundException("Not Found Component for Topic");
            var mapComponentTasks = new List<Task<BorrowIoTComponentInformation>>();
            foreach (var component in componentTopics)
            {
                mapComponentTasks.Add(MapBorrowIoTComponentInformation(component, @class));
            }
            result.AddRange(await Task.WhenAll(mapComponentTasks));
            return result;
        }

        public async Task RegisterIoTForProject(Guid leaderId, IEnumerable<IoTModelRequest> borrowIoTModels)
        {
            var projectId = (await _commonServices.GetProject(leaderId)).Id;
            var componentMasters = _mapper.Map<IEnumerable<ComponentsMaster>>(borrowIoTModels, opts =>
            {
                opts.Items[nameof(ComponentsMaster.MasterId)] = projectId;
                opts.Items[nameof(ComponentsMaster.MasterType)] = ComponentsMasterType.Project;
            });
           
            await CreateReportIoTForProject(leaderId, borrowIoTModels); // create report before to check that report type is exist

            await _unitOfWork.ComponentsMasterRepository.InsertRangeAsync(componentMasters); // create borrow Iot Components
            await _unitOfWork.SaveChangesAsync();


        }
        private async Task<BorrowIoTComponentInformation> MapBorrowIoTComponentInformation(ComponentsMaster component, IPMSClass @class)
        {
            return new BorrowIoTComponentInformation
            {
                Id = component.ComponentId!.Value,
                Name = component.Component.Name,
                Quantity = await _commonServices.GetRemainComponentQuantityOfLecturer(@class.LecturerId!.Value, component.ComponentId!.Value)
            };
        }

        private async Task CreateReportIoTForProject(Guid leaderId, IEnumerable<IoTModelRequest> borrowIoTModels)
        {
            var components = await _unitOfWork.IoTComponentRepository.Get().Where(Ic => borrowIoTModels.Select(bIm => bIm.ComponentId).Contains(Ic.Id)).ToListAsync();
            string content = "";
            foreach (var iotModels in borrowIoTModels)
            {
                // always found Iot Component, have validation above
                content = content + components.FirstOrDefault(c => c.Id.Equals(iotModels.ComponentId))!.Name + " x " + iotModels.Quantity + "\n"; 
            }

            var reportType = await _unitOfWork.ReportTypeRepository.Get()
                        .FirstOrDefaultAsync(rt => rt.Id.Equals(new Guid("552212b8-7899-491c-84a4-a3bf35cc36ad"))); // find report Borrow Iot Component
            
            if (reportType == null )
            {
                throw new DataNotFoundException("Report type Borrow Iot Components not found");
            }

            var report = new Report
            {
                ReporterId = leaderId,
                Title = "Borrow Iot Components",
                Content = content,
                ReportTypeId = reportType.Id 
            };

            await _unitOfWork.ReportRepository.InsertAsync(report);
            await _unitOfWork.SaveChangesAsync();
        }

        
    }
}
