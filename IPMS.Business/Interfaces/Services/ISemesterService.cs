using IPMS.Business.Requests.Semester;
using IPMS.DataAccess.Models;

namespace IPMS.Business.Interfaces.Services
{
    public interface ISemesterService
    {
        IQueryable<Semester> GetSemesters(GetSemesterRequest request);
    }
}
