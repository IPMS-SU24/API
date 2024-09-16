using IPMS.Business.Interfaces.Repositories;
using IPMS.Business.Repository;
using IPMS.DataAccess;
using IPMS.DataAccess.Models;

namespace IPMS.Business.Repositories
{
    public class KitDeviceRepository : GenericRepository<KitDevice>, IKitDeviceRepository
    {
        public KitDeviceRepository(IPMSDbContext context) : base(context)
        {
        }
    }
}
