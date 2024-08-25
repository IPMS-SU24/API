using IPMS.Business.Interfaces.Repositories;
using IPMS.Business.Repository;
using IPMS.DataAccess.Models;
using IPMS.DataAccess;

namespace IPMS.Business.Repositories
{
    public class IPMSClassRepository : GenericRepository<IPMSClass>, IIPMSClassRepository
    {
        public IPMSClassRepository(IPMSDbContext context) : base(context)
        {

        }
    }
}
