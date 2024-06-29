using IPMS.Business.Models;
using IPMS.Business.Requests.SubmissionModule;
using IPMS.Business.Responses.SubmissionModule;

namespace IPMS.Business.Interfaces.Services
{
    public interface ISubmissionModuleService
    {
        public Task<ValidationResultModel> ConfigureSubmissionModuleValidator(ConfigureSubmissionModuleRequest request, Guid currentUserId);
        public Task ConfigureSubmissionModule(ConfigureSubmissionModuleRequest request, Guid currentUserId);
        public Task<ValidationResultModel> GetAssessmentSubmissionModuleByClassValidator(GetSubmissionModuleByClassRequest request, Guid currentUserId);

        public Task<IEnumerable<GetAssessmentSubmissionModuleByClassResponse>> GetAssessmentSubmissionModuleByClass(GetSubmissionModuleByClassRequest request, Guid currentUserId);

    }
}
