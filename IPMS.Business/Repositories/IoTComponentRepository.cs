using IPMS.Business.Interfaces.Repositories;
using IPMS.Business.Repository;
using IPMS.DataAccess;
using IPMS.DataAccess.Models;

namespace IPMS.Business.Repositories
{
    public class IoTComponentRepository : GenericRepository<IoTComponent>, IIoTComponentRepository
    {
        public IoTComponentRepository(IPMSDbContext context) : base(context)
        {

        }
    }
}
