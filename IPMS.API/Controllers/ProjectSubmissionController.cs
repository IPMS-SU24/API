using IPMS.API.Common.Attributes;
using IPMS.API.Common.Enums;
using IPMS.API.Common.Extensions;
using IPMS.API.Responses;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.ProjectSubmission;
using IPMS.Business.Responses.Assessment;
using IPMS.Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace IPMS.API.Controllers
{
    public class ProjectSubmissionController : ApiControllerBase
    {
        private readonly IProjectSubmissionService _projectSubmissionService;
        public ProjectSubmissionController(IProjectSubmissionService projectSubmissionService)
        {
            _projectSubmissionService = projectSubmissionService;
        }
        [EnumAuthorize(UserRole.Student)]
        [HttpPut]
        public async Task<IActionResult> GetProjectSubmissionName([FromBody] UpdateProjectSubmissionRequest request)
        {
            Guid currentUserId = HttpContext.User.Claims.GetUserId();
            var response = new IPMSResponse<bool>
            {
                Data = await _projectSubmissionService.UpdateProjectSubmission(request, currentUserId)

            };

            if (response.Data == false)
            {
                response.Status = ResponseStatus.BadRequest;
            }
            return GetActionResponse(response);
        }
    }
}
