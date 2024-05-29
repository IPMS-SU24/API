using IPMS.Business.Responses.ProjectDashboard;

namespace IPMS.Business.Interfaces.Services
{
    public interface IProjectDashboardService
    {
        Task<GetProjectDetailData> GetProjectDetail(Guid studentId);
        Task<NearSubmissionDeadlineData> GetNearSubmissionDeadlines(Guid studentId);
    }
}
