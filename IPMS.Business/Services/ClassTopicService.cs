using IPMS.Business.Interfaces.Services;
using IPMS.Business.Interfaces;
using IPMS.Business.Requests.ClassTopic;
using IPMS.DataAccess.Models;
using AutoFilterer.Extensions;
using Microsoft.EntityFrameworkCore;

using IPMS.Business.Responses.Topic;
using IPMS.DataAccess.Common.Enums;
using IPMS.Business.Common.Utils;
using System.Linq;

namespace IPMS.Business.Services
{
    public class ClassTopicService : IClassTopicService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonServices _commonServices;
        public ClassTopicService(IUnitOfWork unitOfWork, ICommonServices commonServices)
        {
            _unitOfWork = unitOfWork;
            _commonServices = commonServices;
        }
        public IQueryable<ClassTopic> GetClassTopics(GetClassTopicRequest request)
        {
            return _unitOfWork.ClassTopicRepository.Get().ApplyFilter(request).AsNoTracking();
        }
     
        public async Task<IQueryable<TopicIotComponentReponse>> GetClassTopicsAvailable(Guid currentUserId, GetClassTopicRequest request)
        {
            request.searchValue = request.searchValue.Trim().ToLower();

            // Get current Semester
            Guid currentSemesterId = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester!.Id;

            var studiesInId = (await _commonServices.GetStudiesIn(currentUserId)).Select(s => s.ClassId).ToList();

            if (studiesInId.Count() == 0 || studiesInId == null)
                return null;

            Guid? currentClassId = (await _commonServices.GetCurrentClass(studiesInId, currentSemesterId))?.Id;

            // Check null current user did not enrolled any class this semester
            if (currentClassId == null)
                return null;


            var availableClassTopics = _unitOfWork.ClassTopicRepository.Get() // Find ClassTopics are available and include Topic
                                                                       .Where(ct => ct.ClassId.Equals(currentClassId)
                                                                                    && ct.ProjectId == null
                                                                                    && ct.Topic!.Status == RequestStatus.Approved // Only Topic Approved can choose
                                                                                    && (ct.Topic.Name.ToLower().Contains(request.searchValue) || ct.Topic.Description.ToLower().Contains(request.searchValue)))
                                                                       .            Include(ct => ct.Topic);

            /*
                In TopicIotComponentResponse have ComponentsMaster can query base on MasterType = Topic && MasterId == currentTopicId but we not need to specific these 
                -> Just see that any ComponentMaster Exist 
                -> have IoTComponent -> Get Name + Description

             */

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
        public async Task<bool> PickTopic(Guid currentUserId, Guid topicId)
        {
            Guid currentSemesterId = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester!.Id; // Get current Semester

            var studiesIn = await _commonServices.GetStudiesIn(currentUserId);

            if (studiesIn.Count() == 0 || studiesIn == null)
                return false;

            IPMSClass? currentClass = await _commonServices.GetCurrentClass(studiesIn.Select(s => s.ClassId), currentSemesterId);

            if (currentClass == null) // Check null current user did not enrolled any class this semester
                return false;

            if (currentClass.ChangeTopicDeadline < DateTime.Now) // Check is expired
                return false;

            // Check if project is request topic
            var currentStudiesIn = studiesIn.FirstOrDefault(s => s.ClassId.Equals(currentClass.Id));

            if (currentStudiesIn == null)
                return false;

            ClassTopic? pickedTopic = await _unitOfWork.ClassTopicRepository.Get() // Find ClassTopic picked
                                                                      .Include(ct => ct.Topic).FirstOrDefaultAsync(ct => ct.ClassId.Equals(currentClass.Id)
                                                                      && ct.ProjectId.Equals(currentStudiesIn.ProjectId));
          
            if (pickedTopic?.Topic != null) // Check topic is not null
            {
                if (pickedTopic.Topic.Status != RequestStatus.Approved) // Check if status is in request processing then cannot change topic
                {
                    return false;
                }
            }

            ClassTopic? pickedTopicAvailable = await _unitOfWork.ClassTopicRepository.Get() // Is Picked Topic available
                                                                               .FirstOrDefaultAsync(ct => ct.ClassId.Equals(currentClass.Id)
                                                                               && ct.ProjectId == null && ct.TopicId.Equals(topicId)
                                                                               && ct.Topic!.Status == RequestStatus.Approved); // Only project approved can pick

            if (pickedTopicAvailable == null)
                return false;

            // Set picked topic is available
            if (pickedTopic != null)
            {
                pickedTopic.ProjectId = null;
                _unitOfWork.ClassTopicRepository.Update(pickedTopic);
                _unitOfWork.SaveChanges(); // Save change prevent conflict save in unit of work

            }

            pickedTopicAvailable.ProjectId = currentStudiesIn.ProjectId; //Set that group pick topic
            _unitOfWork.ClassTopicRepository.Update(pickedTopicAvailable); // Update DB
            _unitOfWork.SaveChanges(); // Save change
            return true;
        }

    }

}
