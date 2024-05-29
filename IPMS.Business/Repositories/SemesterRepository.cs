using IPMS.Business.Interfaces.Repositories;
using IPMS.Business.Repository;
using IPMS.DataAccess;
using IPMS.DataAccess.Models;

namespace IPMS.Business.Repositories
{
    public class SemesterRepository : GenericRepository<Semester>, ISemesterRepository
    {
        public SemesterRepository(IPMSDbContext context) : base(context)
        {
        }
    }
}
