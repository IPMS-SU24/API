using IPMS.API.Common.Attributes;
using IPMS.API.Common.Enums;
using IPMS.API.Common.Extensions;
using IPMS.API.Responses;
using IPMS.Business.Common.Enums;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.SubmissionModule;
using IPMS.Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace IPMS.API.Controllers
{
    public class SubmissionModuleController : ApiControllerBase
    {
        private readonly ISubmissionModuleService _submissionModuleService;
        public SubmissionModuleController(ISubmissionModuleService submissionModuleService)
        {
            _submissionModuleService = submissionModuleService;
        }
        [EnumAuthorize(UserRole.Lecturer)]
        [HttpPut]
        public async Task<IActionResult> ConfigureSubmissionModule([FromBody] ConfigureSubmissionModuleRequest request)
        {
            Guid currentUserId = HttpContext.User.Claims.GetUserId();
            await _submissionModuleService.ConfigureSubmissionModule(request, currentUserId);
            return GetActionResponse(new IPMSResponse<object>());
        }
        [EnumAuthorize(UserRole.Lecturer)]
        [HttpGet]
        public async Task<IActionResult> GetAssessmentSubmissionModuleByClass([FromQuery] GetSubmissionModuleByClassRequest request)
        {
            Guid currentUserId = HttpContext.User.Claims.GetUserId();
            var submissionModules = await _submissionModuleService.GetAssessmentSubmissionModuleByClass(request, currentUserId);
            var response = await submissionModules.GetPaginatedResponse(page: request.Page, pageSize: request.PageSize);
            return GetActionResponse(response);
        }
    }
}
