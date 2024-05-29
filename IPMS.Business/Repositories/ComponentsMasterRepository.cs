using IPMS.Business.Interfaces.Repositories;
using IPMS.Business.Repository;
using IPMS.DataAccess.Models;
using IPMS.DataAccess;

namespace IPMS.Business.Repositories
{
    public class ComponentsMasterRepository : GenericRepository<ComponentsMaster>, IComponentsMasterRepository
    {
        public ComponentsMasterRepository(IPMSDbContext dbContext) : base(dbContext)
        {

        }
    }
}
