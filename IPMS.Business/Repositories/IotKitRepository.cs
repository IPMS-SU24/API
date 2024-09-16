using IPMS.Business.Interfaces.Repositories;
using IPMS.Business.Repository;
using IPMS.DataAccess;
using IPMS.DataAccess.Models;

namespace IPMS.Business.Repositories
{
    public class IoTKitRepository : GenericRepository<IoTKit>, IIoTKitRepository
    {
        public IoTKitRepository(IPMSDbContext context) : base(context)
        {
        }
    }
}
