using IPMS.Business.Interfaces.Repositories;
using IPMS.Business.Repository;
using IPMS.DataAccess;
using IPMS.DataAccess.Models;

namespace IPMS.Business.Repositories
{
    public class ClassModuleDealineRepository : GenericRepository<ClassModuleDeadline>, IClassModuleDealineRepository
    {
        public ClassModuleDealineRepository(IPMSDbContext context) : base(context)
        {
        }
    }
}
