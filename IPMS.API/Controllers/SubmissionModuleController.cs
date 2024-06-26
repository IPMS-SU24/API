using IPMS.API.Common.Attributes;
using IPMS.API.Common.Enums;
using IPMS.API.Common.Extensions;
using IPMS.API.Responses;
using IPMS.Business.Common.Enums;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.SubmissionModule;
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
            var response = new IPMSResponse<bool>
            {
                Data = await _submissionModuleService.UpdateSubmissionModule(request, currentUserId)

            };

            if (response.Data == false)
            {
                response.Status = ResponseStatus.BadRequest;
            }
            return GetActionResponse(response);
        }


      
    }
}
