using IPMS.Business.Requests.ProjectPreference;
using IPMS.Business.Responses.ProjectDashboard;
using IPMS.Business.Responses.ProjectPreference;

namespace IPMS.Business.Interfaces.Services
{
    public interface IProjectService
    {
        Task<string?> GetProjectName(Guid currentUserId);
        Task<ProjectProgressData> GetProjectProgressData(Guid currentUserId);
        Task<IEnumerable<ProjectPreferenceResponse>> GetProjectPreferences(ProjectPreferenceRequest request);
    }
}
