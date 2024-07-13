using IPMS.Business.Requests.Semester;
using IPMS.Business.Responses.Semester;

namespace IPMS.Business.Interfaces.Services
{
    public interface ISemesterService
    {
        Task<GetAllSemestersResponse> GetAllSemesters();
        Task<GetCurrentSemesterResponse> GetCurrentSemester();
        Task<GetClassInfoInSemesterResponse> GetClassesInSemester(Guid lecturerId, GetClassInfoInSemesterRequest request);
    }
}
