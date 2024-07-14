using IPMS.Business.Models;
using IPMS.Business.Requests.ProjectSubmission;
using IPMS.Business.Responses.ProjectSubmission;

namespace IPMS.Business.Interfaces.Services
{
    public interface IProjectSubmissionService
    {
        Task<bool> UpdateProjectSubmission(UpdateProjectSubmissionRequest request, Guid currentUserId);
        Task<ValidationResultModel> GradeSubmissionValidators(GradeSubmissionRequest request, Guid lecturerId);
        Task GradeSubmission(GradeSubmissionRequest request, Guid lecturerId);
        Task<IQueryable<GetAllSubmissionResponse>> GetAllSubmission(GetAllSubmissionRequest request, Guid currentUserId);
        Task<ValidationResultModel> UpdateProjectSubmissionValidators(UpdateProjectSubmissionRequest request);
    }
}
