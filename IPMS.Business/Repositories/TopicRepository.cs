using IPMS.Business.Interfaces.Repositories;
using IPMS.Business.Repository;
using IPMS.DataAccess;
using IPMS.DataAccess.Models;
using IPMS.DataAccess;

namespace IPMS.Business.Repositories
{
    public class TopicRepository : GenericRepository<Topic>, ITopicRepository
    {
        public TopicRepository(IPMSDbContext dbContext) : base(dbContext)
        {
        }
    }
}
