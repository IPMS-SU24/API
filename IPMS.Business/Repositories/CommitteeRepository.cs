using IPMS.Business.Interfaces.Repositories;
using IPMS.Business.Repository;
using IPMS.DataAccess;
using IPMS.DataAccess.Models;

namespace IPMS.Business.Repositories
{
    public class CommitteeRepository : GenericRepository<Committee>, ICommitteeRepository
    {
        public CommitteeRepository(IPMSDbContext context) : base(context)
        {
        }
    }
}
