using IPMS.Business.Interfaces.Repositories;
using IPMS.Business.Repository;
using IPMS.DataAccess;
using IPMS.DataAccess.Models;

namespace IPMS.Business.Repositories
{
    public class ClassModuleDeadlineRepository : GenericRepository<ClassModuleDeadline>, IClassModuleDeadlineRepository
    {
        public ClassModuleDeadlineRepository(IPMSDbContext context) : base(context)
        {
        }
    }
}
