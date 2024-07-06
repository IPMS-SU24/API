using IPMS.Business.Common.Utils;
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
using Microsoft.AspNetCore.Http;
using System.Xml.Linq;
using IPMS.Business.Common.Extensions;
using IPMS.Business.Responses.IoT;
using IPMS.Business.Responses.Project;

namespace IPMS.Business.Services
{
    public class BorrowIoTService : IBorrowIoTService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonServices _commonServices;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _context;

        public BorrowIoTService(IUnitOfWork unitOfWork, ICommonServices commonServices, IMapper mapper, IHttpContextAccessor context)
        {
            _unitOfWork = unitOfWork;
            _commonServices = commonServices;
            _mapper = mapper;
            _context = context;
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
            var topicId = await _unitOfWork.ClassTopicRepository.Get().Where(x => x.ProjectId == project.Id).Select(x => x.TopicId).FirstOrDefaultAsync(); //checked project get topic
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
            var componentTopics = await _unitOfWork.ComponentsMasterRepository.GetTopicComponents().Where(x => x.MasterId == request.TopicId).Include(x => x.Component).ToListAsync();
            if (!componentTopics.Any()) throw new DataNotFoundException("Not Found Component for Topic");
            var mapComponentTasks = new List<Task<BorrowIoTComponentInformation>>();
            foreach (var component in componentTopics)
            {
                mapComponentTasks.Add(MapBorrowIoTComponentInformation(component, @class));
            }
            result.AddRange(await Task.WhenAll(mapComponentTasks));
            return result;
        }

        public async Task<IEnumerable<GetBorrowIoTComponentsResponse>> GetBorrowIoTComponents(GetBorrowIoTComponentsRequest request, Guid lecturerId)
        {
            var currentSemester = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester;

            var classes = await _unitOfWork.IPMSClassRepository.Get().Where(c => c.LecturerId.Equals(lecturerId)
                                                && c.Semester.Id.Equals(currentSemester.Id))
                                            .Include(c => c.Topics)
                                            .ThenInclude(ct => ct.Project)
                                            .ToListAsync();

            if (request.ClassIds.Count != 0) // filter
            {
                classes = classes.Where(c => request.ClassIds.Contains(c.Id)).ToList();
            }


            var components = await _unitOfWork.ComponentsMasterRepository.Get() // get components
                                    .Where(cm => cm.MasterType == ComponentsMasterType.Project)
                                    .Include(cm => cm.Component).ToListAsync();


            List<ProjectInformation> projects = new List<ProjectInformation>();
            foreach (var @class in classes) // prepare information
            {
                projects.AddRange(@class.Topics.Where(ct => ct.ProjectId != null).Select(ct => new ProjectInformation
                {
                    ProjectId = ct.ProjectId,
                    GroupName = ct.Project.GroupName,
                    ClassName = @class.Name
                }).ToList());
            }

            List<GetBorrowIoTComponentsResponse> borrowComponents = new();
            
            foreach (var prj in projects)
            {
                var prjComponents = components.Where(c => c.MasterId.Equals(prj.ProjectId)); // get borrow component base on projects

                borrowComponents.AddRange( prjComponents.GroupBy(cm => new { cm.CreatedAt.Year, cm.CreatedAt.Month, cm.CreatedAt.Day, cm.CreatedAt.Hour }) // add to response
                .Select(g => new GetBorrowIoTComponentsResponse
                {
                    ClassName = prj.ClassName,
                    GroupName = prj.GroupName,
                    CreateAt = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day),
                    Items = g.Select(g => new IotItem
                    {
                        Id = g.Id,
                        Name = g.Component!.Name,
                        Quantity = g.Quantity,
                        Status = g.Status
                        
                    }).ToList()

                }).ToList());
            }
            if (request.OrderBy)
            {
                borrowComponents = borrowComponents.OrderByDescending(bC => bC.CreateAt).ToList();
            } else
            {
                borrowComponents = borrowComponents.OrderBy(bC => bC.CreateAt).ToList();
            }

            return borrowComponents;
        }

        public async Task<IEnumerable<ReportIoTComponentInformation>> GetGetReportIoTComponents()
        {
            IEnumerable<ReportIoTComponentInformation> report = new List<ReportIoTComponentInformation>();
            Project? project = _context.HttpContext.Session.GetObject<Project?>("Project");

            if (project == null)
            {
                return report;
            }
            var components = await _unitOfWork.ComponentsMasterRepository.Get()
                                    .Where(cm => cm.MasterType == ComponentsMasterType.Project && cm.MasterId.Equals(project.Id))
                                    .Include(cm => cm.Component).ToListAsync();


            report = components.GroupBy(cm => new { cm.CreatedAt.Year, cm.CreatedAt.Month, cm.CreatedAt.Day, cm.CreatedAt.Hour })
                .Select(g => new ReportIoTComponentInformation
                {
                    CreatedAt = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day),
                    IoTComponents = g.Select(g => new BorrowIoTComponentInformation
                    {
                        Id = g.Id,
                        Name = g.Component!.Name,
                        Quantity = g.Quantity
                    }).ToList()

                }).ToList();
            return report;
        }

        public async Task RegisterIoTForProject(Guid leaderId, IEnumerable<IoTModelRequest> borrowIoTModels)
        {
            var projectId = (await _commonServices.GetProject(leaderId)).Id;
            var componentMasters = _mapper.Map<IEnumerable<ComponentsMaster>>(borrowIoTModels, opts =>
            {
                opts.Items[nameof(ComponentsMaster.MasterId)] = projectId;
                opts.Items[nameof(ComponentsMaster.MasterType)] = ComponentsMasterType.Project;
            });

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

    }
}
