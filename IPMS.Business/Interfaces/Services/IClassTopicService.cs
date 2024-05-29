using IPMS.Business.Requests.ClassTopic;
using IPMS.Business.Responses.ClassTopic;
using IPMS.Business.Responses.Topic;
using IPMS.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPMS.Business.Interfaces.Services
{
    public interface IClassTopicService
    {
        IQueryable<ClassTopic> GetClassTopics(GetClassTopicRequest request);
        Task<IQueryable<TopicIotComponentReponse>> GetClassTopicsAvailable(Guid currentUserId, GetClassTopicRequest request);
        Task<bool> PickTopic(Guid currentUserId, Guid topicId);
    }
}
