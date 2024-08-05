using IPMS.Business.Models;
using IPMS.Business.Requests.SubmissionModule;
using IPMS.Business.Responses.SubmissionModule;

namespace IPMS.Business.Interfaces.Services
{
    public interface ISubmissionModuleService
    {
        public Task<ValidationResultModel> ConfigureSubmissionModuleValidator(ConfigureSubmissionModuleRequest request, Guid currentUserId);
        public Task ConfigureSubmissionModule(ConfigureSubmissionModuleRequest request, Guid currentUserId);
        public Task<ValidationResultModel> LecturerEvaluateValidator(LecturerEvaluateRequest request, Guid lecturerId);
        public Task LecturerEvaluate(LecturerEvaluateRequest request, Guid lecturerId);
        public Task<ValidationResultModel> GetAssessmentSubmissionModuleByClassValidator(GetSubmissionModuleByClassRequest request, Guid currentUserId);
        public Task<ValidationResultModel> CalcFinalGradeValidator(CalcFinalGradeRequest request, Guid lecturerId);
        public Task CalcFinalGrade(CalcFinalGradeRequest request, Guid lecturerId);
        public Task<IEnumerable<GetAssessmentSubmissionModuleByClassResponse>> GetAssessmentSubmissionModuleByClass(GetSubmissionModuleByClassRequest request, Guid currentUserId);
        public Task<IEnumerable<GetSubmissionsResponse>> GetSubmissions(GetSubmissionsRequest request, Guid lecturerId);
        public Task<ValidationResultModel> ConfigureClassModuleDeadlineValidators(ConfigureClassModuleDeadlineRequest request, Guid lecturerId);
        public Task ConfigureClassModuleDeadline(ConfigureClassModuleDeadlineRequest request, Guid lecturerId);

    }
}
