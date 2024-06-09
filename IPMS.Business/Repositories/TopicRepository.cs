using IPMS.Business.Interfaces.Repositories;
using IPMS.Business.Repository;
using IPMS.DataAccess;
using IPMS.DataAccess.Models;
using IPMS.DataAccess.Common.Enums;

namespace IPMS.Business.Repositories
{
    public class TopicRepository : GenericRepository<Topic>, ITopicRepository
    {
        public TopicRepository(IPMSDbContext context) : base(context)
        {
        }

        public IQueryable<Topic> GetApprovedTopics()
        {
            return Get().Where(x=>x.Status == RequestStatus.Approved);
        }
    }
}
