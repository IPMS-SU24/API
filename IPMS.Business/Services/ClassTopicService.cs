using IPMS.Business.Interfaces.Services;
using IPMS.Business.Interfaces;
using IPMS.Business.Requests.ClassTopic;
using IPMS.DataAccess.Models;
using AutoFilterer.Extensions;
using Microsoft.EntityFrameworkCore;

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

        public IQueryable<ClassTopic> GetClassTopicsAvailable(Guid currentUserId, GetClassTopicRequest request)
        {
            // Get current Semester
            var currentSemesterId = new Guid("54316eff-6077-425c-9c21-51ed06f615d8");

            // Find Student from current User -> Class repo -> Class Service 
            var listLearns = _unitOfWork.StudentRepository.Get().Where(s => s.InformationId.Equals(currentUserId)).Select(l => l.ClassId); //get list id of learned

            if (listLearns.Count() == 0 || listLearns == null)
                return null;

            //get class that student learned and find in current semester
            IPMSClass? currentClass = _unitOfWork.IPMSClassRepository.Get().Where(c => listLearns.Contains(c.Id) && c.SemesterId.Equals(currentSemesterId)).FirstOrDefault();

            if (currentClass == null)
                return null;
       

            // Find ClassTopics are available
            return _unitOfWork.ClassTopicRepository.Get().Where(ct => ct.ClassId.Equals(currentClass.Id) && ct.ProjectId == null);
        }

    }
}
