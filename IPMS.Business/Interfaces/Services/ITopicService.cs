using IPMS.Business.Requests.Topic;
using IPMS.DataAccess.Models;

namespace IPMS.Business.Interfaces.Services
{
    public interface ITopicService
    {
        IQueryable<Topic> GetTopics(GetTopicRequest request);
    }
}
