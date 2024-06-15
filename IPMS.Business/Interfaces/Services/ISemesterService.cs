using IPMS.Business.Responses.Semester;

namespace IPMS.Business.Interfaces.Services
{
    public interface ISemesterService
    {
        Task<IEnumerable<GetAllSemestersResponse>> GetAllSemesters();
    }
}
