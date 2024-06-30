using IPMS.Business.Requests.Project;
using IPMS.Business.Requests.ProjectPreference;
using IPMS.Business.Responses.Project;
using IPMS.Business.Responses.ProjectDashboard;
using IPMS.Business.Responses.ProjectPreference;

namespace IPMS.Business.Interfaces.Services
{
    public interface IProjectService
    {
        Task<string?> GetProjectName(Guid currentUserId);
        Task<ProjectProgressData> GetProjectProgressData(Guid currentUserId);
        Task<IEnumerable<ProjectPreferenceResponse>> GetProjectPreferences(ProjectPreferenceRequest request);
        Task<IEnumerable<GetProjectsOverviewResponse>> GetProjectsOverview(GetProjectsOverviewRequest request, Guid currentUserId);

    }
}
