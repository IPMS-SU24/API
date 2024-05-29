using IPMS.Business.Interfaces.Repositories;
using IPMS.Business.Repository;
using IPMS.DataAccess;
using IPMS.DataAccess.Models;

namespace IPMS.Business.Repositories
{
    public class AssessmentRepository : GenericRepository<Assessment>, IAssessmentRepository
    {
        public AssessmentRepository(IPMSDbContext context) : base(context)
        {
        }
    }
}
