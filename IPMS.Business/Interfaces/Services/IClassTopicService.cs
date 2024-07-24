using IPMS.Business.Models;
using IPMS.Business.Requests.ClassTopic;
using IPMS.Business.Responses.Topic;
using IPMS.DataAccess.Models;

namespace IPMS.Business.Interfaces.Services
{
    public interface IClassTopicService
    {
        IQueryable<ClassTopic> GetClassTopics(GetClassTopicRequest request);
        Task<IQueryable<TopicIotComponentReponse>> GetClassTopicsAvailable(Guid currentUserId, GetClassTopicRequest request);
        Task<IList<LecturerTopicIotComponentReponse>> GetClassTopicsByLecturer(Guid currentUserId, LecturerClassTopicRequest request);
        Task<bool> PickTopic(Guid topicId);
        Task<ValidationResultModel> PickTopicValidators(Guid topicId);
    }
}
