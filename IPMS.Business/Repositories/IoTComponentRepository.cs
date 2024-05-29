using IPMS.Business.Interfaces.Repositories;
using IPMS.Business.Repository;
using IPMS.DataAccess.Models;
using IPMS.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPMS.Business.Repositories
{
    public class IoTComponentRepository : GenericRepository<IoTComponent>, IIoTComponentRepository
    {
        public IoTComponentRepository(IPMSDbContext dbContext) : base(dbContext)
        {

        }
    }
}
