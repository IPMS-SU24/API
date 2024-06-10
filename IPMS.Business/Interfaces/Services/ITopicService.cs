using IPMS.Business.Models;
using IPMS.Business.Requests.Topic;
using IPMS.DataAccess.Models;

namespace IPMS.Business.Interfaces.Services
{
    public interface ITopicService
    {
        IQueryable<Topic> GetAllTopics();
        IQueryable<Topic> GetApprovedTopics(GetTopicRequest request);
        Task RegisterTopic(RegisterTopicRequest request, Guid leaderId);
        Task<ValidationResultModel> CheckRegisterValid(RegisterTopicRequest request, Guid leaderId);
    }
}
