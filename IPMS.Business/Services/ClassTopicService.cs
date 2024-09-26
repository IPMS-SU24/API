using IPMS.Business.Interfaces.Services;
using IPMS.Business.Interfaces;
using IPMS.Business.Requests.ClassTopic;
using IPMS.DataAccess.Models;
using AutoFilterer.Extensions;
using Microsoft.EntityFrameworkCore;
using IPMS.Business.Responses.Topic;
using IPMS.DataAccess.Common.Enums;
using IPMS.Business.Common.Utils;
using Microsoft.AspNetCore.Http;
using IPMS.Business.Models;
using IPMS.Business.Common.Extensions;
using IPMS.Business.Common.Exceptions;
using IPMS.Business.Requests.Topic;

namespace IPMS.Business.Services
{
    public class ClassTopicService : IClassTopicService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonServices _commonServices;
        private readonly IHttpContextAccessor _context;

        public ClassTopicService(IUnitOfWork unitOfWork, ICommonServices commonServices, IHttpContextAccessor context)
        {
            _unitOfWork = unitOfWork;
            _commonServices = commonServices;
            _context = context;
        }
        public IQueryable<ClassTopic> GetClassTopics(GetClassTopicRequest request)
        {
            return _unitOfWork.ClassTopicRepository.Get().ApplyFilter(request).AsNoTracking();
        }

        public async Task<IQueryable<TopicIotComponentReponse>> GetClassTopicsAvailable(Guid currentUserId, GetClassTopicRequest request)
        {
            if (request.SearchValue == null)
            {
                request.SearchValue = "";
            }

            IPMSClass? @class = _commonServices.GetClass();
            // Check null current user did not enrolled any class this semester
            if (@class == null)
            {
                throw new DataNotFoundException("Not found class");
            }

            Guid? currentClassId = @class?.Id;




            var availableClassTopics = _unitOfWork.ClassTopicRepository.Get().Include(ct => ct.Topic)// Find ClassTopics are available and include Topic
                                                                       .Where(ct => ct.ClassId.Equals(currentClassId)
                                                                                    && ct.ProjectId == null
                                                                                    && ct.Topic!.Status == RequestStatus.Approved // Only Topic Approved can choose
                                                                                    && (ct.Topic.Name.ToLower().Contains(request.SearchValue) || ct.Topic.Description.ToLower().Contains(request.SearchValue)));

            /*
                In TopicIotComponentResponse have ComponentsMaster can query base on MasterType = Topic && MasterId == currentTopicId but we not need to specific these 
                -> Just see that any ComponentMaster Exist 
                -> have IoTComponent -> Get Name + Description

             */

            if (request.AssessmentId.HasValue)
            {
                availableClassTopics = availableClassTopics.Where(x => x.AssessmentId.HasValue && x.AssessmentId == request.AssessmentId);
            }
            else
            {
                availableClassTopics = availableClassTopics.Where(x=>!x.AssessmentId.HasValue);
            }

            var componentsOfTopic = _unitOfWork.ComponentsMasterRepository.Get() // Get component that just for topic
                                                                                .Where(cm => cm.MasterType == ComponentsMasterType.Topic);

            var responses = availableClassTopics.Select(ct => new TopicIotComponentReponse
            {
                Id = ct.Topic!.Id,
                TopicName = ct.Topic.Name,
                Description = ct.Topic.Description,
                Detail = ct.Topic.Detail,
                IotComponents = componentsOfTopic
                        .Where(cm => cm.MasterId.Equals(ct.TopicId))
                        .Select(cm => cm.Component!.Name).ToList()
            });

            return responses;
        }
        public async Task<ValidationResultModel> PickTopicValidators(PickTopicRequest request)
        {
            var result = new ValidationResultModel
            {
                Message = "Operation did not successfully"
            };
            var topicId = request.TopicId;
            Topic? topic = await _unitOfWork.TopicRepository.Get().FirstOrDefaultAsync(t => t.Id.Equals(topicId)); // check topic valid
            if (topic == null)
            {
                result.Message = ("Topic is not exist");
                return result;
            }

            IPMSClass? @class = _context.HttpContext.Session.GetObject<IPMSClass?>("Class");
            if (@class == null)
            {
                result.Message = ("Student did not enrolled any class this semester");
                return result;
            }

            Project? project = _context.HttpContext.Session.GetObject<Project?>("Project");
            if (project == null)
            {
                result.Message = ("Student not in any project currently");
                return result;

            }
            ClassTopic? pickedTopic = await _unitOfWork.ClassTopicRepository.Get() // Find ClassTopic picked
                                                                      .Include(ct => ct.Topic).FirstOrDefaultAsync(ct => ct.ClassId.Equals(@class.Id)
                                                                      && ct.ProjectId.Equals(project.Id));
            var isMultipleTopic = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester!.IsMultipleTopic;
            if(isMultipleTopic)
            {
                if(!request.AssessmentId.HasValue)
                {
                    result.Message = "Assessment cannot be empty";
                    return result;
                }
                var assessment = await _unitOfWork.AssessmentRepository.Get().FirstOrDefaultAsync(x => x.Id == request.AssessmentId);
                if (assessment == null)
                {
                    result.Message = "Not Found Assessment";
                    return result;
                }
                var assessmentTime = await _commonServices.GetAssessmentTime(assessment.Id, @class.Id);
                if(assessmentTime.endDate < DateTime.Now)
                {
                    result.Message = "Cannot change topic at this time";
                    return result;
                }
            }
            else
            {
                if (@class.ChangeTopicDeadline < DateTime.Now) // Check is expired 
                {
                    result.Message = ("Cannot change topic at this time");
                    return result;
                }

                if (pickedTopic?.Topic != null) // Check topic is not null
                {
                    if (pickedTopic.Topic.Status != RequestStatus.Approved) // Check if status is in request processing then cannot change topic
                    {
                        result.Message = ("Group has a project waiting to be approved");
                        return result;
                    }
                }
            }

            var pickedTopicAvailableQuery = _unitOfWork.ClassTopicRepository.Get() // Is Picked Topic available
                                                                               .Where(ct => ct.ClassId.Equals(@class.Id)
                                                                               && ct.ProjectId == null && ct.TopicId.Equals(topicId)
                                                                               && ct.Topic.Status == RequestStatus.Approved); // Only project approved can pick

            ClassTopic? pickedTopicAvailable = isMultipleTopic
                ? await pickedTopicAvailableQuery.FirstOrDefaultAsync(x => x.AssessmentId == request.AssessmentId) 
                : await pickedTopicAvailableQuery.FirstOrDefaultAsync();
            
            if (pickedTopicAvailable == null)
            {
                result.Message = ("This topic is not available");
                return result;
            }
            result.Message = string.Empty;
            result.Result = true;
            return result;
        }
        public async Task<bool> PickTopic(PickTopicRequest request)
        {
            var topicId = request.TopicId;
            IPMSClass? @class = _context.HttpContext.Session.GetObject<IPMSClass?>("Class");
            Project? project = _context.HttpContext.Session.GetObject<Project?>("Project");

            var isMultipleTopic = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester!.IsMultipleTopic;

            var pickedTopicQuery = _unitOfWork.ClassTopicRepository.Get() // Find ClassTopic picked
                                                                      .Where(ct => ct.ClassId.Equals(@class.Id)
                                                                      && ct.ProjectId.Equals(project.Id));
            var pickedTopic = isMultipleTopic 
                ? await pickedTopicQuery.FirstOrDefaultAsync(x => x.AssessmentId == request.AssessmentId) 
                : await pickedTopicQuery.FirstOrDefaultAsync(); 

            var pickedTopicAvailableQuery = _unitOfWork.ClassTopicRepository.Get() // Is Picked Topic available
                                                                               .Where(ct => ct.ClassId.Equals(@class.Id)
                                                                               && ct.ProjectId == null && ct.TopicId.Equals(topicId)
                                                                               && ct.Topic.Status == RequestStatus.Approved); // Only project approved can pick

            ClassTopic? pickedTopicAvailable = isMultipleTopic
                ? await pickedTopicAvailableQuery.FirstOrDefaultAsync(x => x.AssessmentId == request.AssessmentId)
                : await pickedTopicAvailableQuery.FirstOrDefaultAsync();

            // Set picked topic is available
            if (pickedTopic != null)
            {
                pickedTopic.ProjectId = null;
                _unitOfWork.ClassTopicRepository.Update(pickedTopic);
                _unitOfWork.SaveChanges(); // Save change prevent conflict save in unit of work

            }

            pickedTopicAvailable.ProjectId = project.Id; //Set that group pick topic
            _unitOfWork.ClassTopicRepository.Update(pickedTopicAvailable); // Update DB
            _unitOfWork.SaveChanges(); // Save change
            return true;
        }

        public async Task<IList<LecturerTopicIotComponentReponse>> GetClassTopicsByLecturer(Guid currentUserId, LecturerClassTopicRequest request)
        {
            IQueryable<ClassTopic> availableClassTopics = _unitOfWork.ClassTopicRepository.Get() // Find ClassTopics are available and include Topic
                                                                      .Where(ct => ct.ClassId.Equals(request.ClassId) &&
                                                                                   ct.Topic!.Status == RequestStatus.Approved // Only Topic Approved can choose
                                                                                                                                 )
                                                                      .Include(ct => ct.Topic);

            if (request.IsCommittee.HasValue && request.IsCommittee.Value)
            {
                availableClassTopics = availableClassTopics.Include(x => x.Class).ThenInclude(x=>x.Committees).Where(ct => ct.Class.LecturerId != currentUserId && ct.Class.Committees.Any(x=>x.LecturerId == currentUserId));
            }
            else
            {
                availableClassTopics = availableClassTopics.Where(ct => ct.Class.LecturerId == currentUserId);
            }
            /*
                In TopicIotComponentResponse have ComponentsMaster can query base on MasterType = Topic && MasterId == currentTopicId but we not need to specific these 
                -> Just see that any ComponentMaster Exist 
                -> have IoTComponent -> Get Name + Description

             */

            var componentsOfTopic = _unitOfWork.ComponentsMasterRepository.Get() // Get component that just for topic
                                                                                .Where(cm => cm.MasterType == ComponentsMasterType.Topic);
            var responses = await availableClassTopics.Select(ct => new LecturerTopicIotComponentReponse
            {
                Id = ct.Topic!.Id,
                TopicName = ct.Topic.Name,
                Description = ct.Topic.Description,
                Detail = ct.Topic.Detail,
                IotComponents = componentsOfTopic
                        .Where(cm => cm.MasterId.Equals(ct.TopicId))
                        .Select(cm => cm.Component!.Name).ToList(),
                PickedBy = ct.ProjectId,
                AssessmentName = ct.AssessmentId.HasValue ? ct.Assessment!.Name : null,
                AssessmentId = ct.AssessmentId

            }).ToListAsync();
            if (responses == null || !responses.Any()) throw new DataNotFoundException("Not found any topic or class is not belong to lecturer");
            return responses;
        }
    }

}
