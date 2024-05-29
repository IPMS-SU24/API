using IPMS.Business.Interfaces.Repositories;
using IPMS.Business.Repository;
using IPMS.DataAccess;
using IPMS.DataAccess.Models;

namespace IPMS.Business.Repositories
{
    public class IPMSClassRepository : GenericRepository<IPMSClass>, IIPMSClassRepository
    {
        public IPMSClassRepository(IPMSDbContext context) : base(context)
        {
        }
        private void Test()
        {
            var a = Get().FirstOrDefault();
        }
    }
}
