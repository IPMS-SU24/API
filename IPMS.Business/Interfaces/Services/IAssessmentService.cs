using IPMS.Business.Models;
using IPMS.Business.Requests.Assessment;
using IPMS.Business.Responses.Assessment;

namespace IPMS.Business.Interfaces.Services
{
    public interface IAssessmentService
    {
        Task<AssessmentSubmissionProjectResponse> GetAssessmentById(Guid assessmentId);
        Task<IEnumerable<GetAllAssessmentsResponse>> GetAllAssessments(GetAllAssessmentsRequest request);
        Task<ValidationResultModel> GetAssessmentByIdValidators(Guid assessmentId);
    }
}
