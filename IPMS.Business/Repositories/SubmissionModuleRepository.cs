using IPMS.Business.Interfaces.Repositories;
using IPMS.Business.Repository;
using IPMS.DataAccess;
using IPMS.DataAccess.Models;

namespace IPMS.Business.Repositories
{
    public class SubmissionModuleRepository : GenericRepository<SubmissionModule>, ISubmissionModuleRepository
    {
        public SubmissionModuleRepository(IPMSDbContext context) : base(context)
        {
        }
    }
}
