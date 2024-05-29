using IPMS.Business.Interfaces.Repositories;
using IPMS.Business.Repository;
using IPMS.DataAccess;
using IPMS.DataAccess.Models;

namespace IPMS.Business.Repositories
{
    public class ClassTopicRepository : GenericRepository<ClassTopic>, IClassTopicRepository
    {
        public ClassTopicRepository(IPMSDbContext context) : base(context)
        {

        }
    }
}
