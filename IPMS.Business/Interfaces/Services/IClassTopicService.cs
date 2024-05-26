using IPMS.Business.Requests.ClassTopic;
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
        IQueryable<ClassTopic> GetClassTopicsAvailable(Guid currentUserId, GetClassTopicRequest request);
    }
}
