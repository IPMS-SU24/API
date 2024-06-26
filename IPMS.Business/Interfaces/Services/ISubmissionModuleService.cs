using IPMS.Business.Models;
using IPMS.Business.Requests.SubmissionModule;

namespace IPMS.Business.Interfaces.Services
{
    public interface ISubmissionModuleService
    {
        public Task<ValidationResultModel> ConfigureSubmissionModuleValidator(ConfigureSubmissionModuleRequest request, Guid currentUserId);
        public Task ConfigureSubmissionModule(ConfigureSubmissionModuleRequest request, Guid currentUserId);
    }
}
