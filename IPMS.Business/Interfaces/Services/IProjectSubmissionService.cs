using IPMS.Business.Requests.ProjectSubmission;
using IPMS.Business.Responses.ProjectSubmission;

namespace IPMS.Business.Interfaces.Services
{
    public interface IProjectSubmissionService
    {
        Task<bool> UpdateProjectSubmission(UpdateProjectSubmissionRequest request, Guid currentUserId);
        Task<IQueryable<GetAllSubmissionResponse>> GetAllSubmission(GetAllSubmissionRequest request, Guid currentUserId);
    }
}
