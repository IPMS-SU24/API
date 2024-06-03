
using IPMS.Business.Requests.ProjectSubmission;

namespace IPMS.Business.Interfaces.Services
{
    public interface IProjectSubmissionService
    {
        Task<bool> UpdateProjectSubmission(UpdateProjectSubmissionRequest request, Guid currentUserId);
    }
}
