using IPMS.Business.Responses.ProjectDashboard;

namespace IPMS.Business.Interfaces.Services
{
    public interface IProjectService
    {
        public Task<string?> GetProjectName(Guid currentUserId);
        public Task<ProjectProgressData> GetProjectProgressData(Guid currentUserId);
    }
}
