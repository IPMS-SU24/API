using IPMS.Business.Responses;

namespace IPMS.Business.Interfaces.Services
{
    public interface IProjectDashboardService
    {
        Task<GetProjectDetailData> GetProjectDetail(Guid studentId);
    }
}
