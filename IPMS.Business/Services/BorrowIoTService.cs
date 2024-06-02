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

        public async Task<bool> CheckIoTValid(BorrowIoTModelRequest request, Guid leaderId)
        {
            if (request.Quantity <= 0) return false;
            var currentSemester = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester;
            var studiesIn = await _commonServices.GetStudiesIn(leaderId);
            var project = await _commonServices.GetProject(leaderId);
            var @class = await _commonServices.GetCurrentClass(studiesIn.Select(x=>x.ClassId), currentSemester.Id);
            //Check borrow Assessment Status => Not In Progress => Validate Fail
            var borrowAssessmentStatus = await _commonServices.GetBorrowIoTStatus(project.Id, @class);
            if (borrowAssessmentStatus != AssessmentStatus.InProgress) return false;
            //Check Exist
            var iot = await _unitOfWork.IoTComponentRepository.GetByID(request.ComponentId);
            if (iot == null) return false;
            //Check iot in iot list of Topic
            var topicId = await _unitOfWork.ClassTopicRepository.Get().Where(x=>x.ProjectId == project.Id).Select(x=>x.TopicId).FirstOrDefaultAsync();
            var isInTopicComponent = await _unitOfWork.ComponentsMasterRepository.GetTopicComponents()
                                                                                    .Where(x => x.MasterId == topicId && x.ComponentId == request.ComponentId).AnyAsync();
            if (!isInTopicComponent) return false;
            //Check remain Quantity
            var remainQuantity = await _commonServices.GetRemainComponentQuantityOfLecturer(@class.LecturerId!.Value, request.ComponentId);
            if(remainQuantity < request.Quantity) return false;
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
            foreach (var component in componentTopics)
            {
                var info = new BorrowIoTComponentInformation
                {
                    Id = component.ComponentId!.Value,
                    Name = component.Component.Name,
                    Quantity = await _commonServices.GetRemainComponentQuantityOfLecturer(@class.LecturerId!.Value, component.ComponentId!.Value)
                };
                result.Add(info);
            }
            return result;
        }

        public async Task RegisterIoTForProject(Guid leaderId, IEnumerable<BorrowIoTModelRequest> borrowIoTModels)
        {
            var projectId = (await _commonServices.GetProject(leaderId)).Id;
            var componentMasters = _mapper.Map<IEnumerable<ComponentsMaster>>(borrowIoTModels, opts =>
            {
                opts.Items["MasterId"] = projectId;
            });
            await _unitOfWork.ComponentsMasterRepository.InsertRange(componentMasters);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
