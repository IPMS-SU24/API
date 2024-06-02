using IPMS.Business.Common.Utils;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests;
using IPMS.DataAccess.Models;
using IPMS.Business.Common.Enums;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

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

        public async Task RegisterIoTForProject(Guid leaderId, IEnumerable<BorrowIoTModelRequest> borrowIoTModels)
        {
            var projectId = (await _commonServices.GetProject(leaderId)).Id;
            var componentMasters = _mapper.Map<IEnumerable<ComponentsMaster>>(borrowIoTModels, opts =>
            {
                opts.Items["MasterId"] = projectId;
            });
            await _unitOfWork.ComponentsMasterRepository.InsertRange(componentMasters);
        }
    }
}
