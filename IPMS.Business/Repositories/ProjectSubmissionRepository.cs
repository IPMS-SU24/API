using IPMS.Business.Interfaces.Repositories;
using IPMS.Business.Repository;
using IPMS.DataAccess;
using IPMS.DataAccess.Models;

namespace IPMS.Business.Repositories
{
    public class ProjectSubmissionRepository : GenericRepository<ProjectSubmission>, IProjectSubmissionRepository
    {
        public ProjectSubmissionRepository(IPMSDbContext context) : base(context)
        {
        }
    }
}
