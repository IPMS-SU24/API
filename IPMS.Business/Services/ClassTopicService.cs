using IPMS.Business.Interfaces.Services;
using IPMS.Business.Interfaces;
using IPMS.Business.Requests.ClassTopic;
using IPMS.DataAccess.Models;
using AutoFilterer.Extensions;
using Microsoft.EntityFrameworkCore;

using IPMS.Business.Responses.Topic;
using IPMS.DataAccess.Common.Enums;
using IPMS.Business.Common.Utils;

namespace IPMS.Business.Services
{
    public class ClassTopicService : IClassTopicService
    {
        private readonly IUnitOfWork _unitOfWork;
        public ClassTopicService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IQueryable<ClassTopic> GetClassTopics(GetClassTopicRequest request)
        {
            return _unitOfWork.ClassTopicRepository.Get().ApplyFilter(request).AsNoTracking();
        }

        public async Task<IQueryable<TopicIotComponentReponse>> GetClassTopicsAvailable(Guid currentUserId, GetClassTopicRequest request)
        {
            // Get current Semester
            var currentSemesterId = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester.Id;
            
            var studyIn = _unitOfWork.StudentRepository.Get() // Find Student from current User 
                .Where(s => s.InformationId.Equals(currentUserId))
                .Select(s => s.ClassId);

            if (studyIn.Count() == 0 || studyIn == null)
                return null;

            var currentClassId = _unitOfWork.IPMSClassRepository.Get() // Get class that student learned and find in current semester
                .Where(c => studyIn.Contains(c.Id) 
                && c.SemesterId.Equals(currentSemesterId))
                .Select(c => c.Id).FirstOrDefault();

            // Check null current user did not enrolled any class this semsester
            if (currentClassId == null)
                return null;

           
            var availableClassTopics = _unitOfWork.ClassTopicRepository.Get() // Find ClassTopics are available and include Topic
                    .Where(ct => ct.ClassId.Equals(currentClassId) 
                    && ct.ProjectId == null).Include(ct => ct.Topic);

            /*
                In TopicIotComponentReponse have ComponentsMaster can query base on MasterType = Topic && MasterId == currentTopicId but we not need to specific these 
                -> Just see that any ComponentMaster Exist 
                -> have IoTComponent -> Get Name + Description

             */
            
            var componentsOfTopic = _unitOfWork.ComponentsMasterRepository.Get() // Get component that just for topic
                .Where(cm => cm.MasterType == ComponentsMasterType.Topic);

            var responses = availableClassTopics.Select(ct => new TopicIotComponentReponse
            {
                Id = ct.Topic.Id,
                TopicName = ct.Topic.Name,
                Description = ct.Topic.Description,
                Detail = ct.Topic.Detail,
                IotComponents = componentsOfTopic
                        .Where(cm => cm.MasterId.Equals(ct.TopicId))
                        .Select(cm => cm.Component.Name).ToList()
            });
         
            return responses;
        }

    }
}
