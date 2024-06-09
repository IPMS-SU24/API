using IPMS.Business.Interfaces.Repositories;
using IPMS.Business.Repository;
using IPMS.DataAccess;
using IPMS.DataAccess.Models;

namespace IPMS.Business.Repositories
{
    public class MemberHistoryRepository : GenericRepository<MemberHistory>, IMemberHistoryRepository
    {
        public MemberHistoryRepository(IPMSDbContext context) : base(context)
        {
        }
    }
}
