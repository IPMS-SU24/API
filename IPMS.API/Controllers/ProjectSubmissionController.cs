using IPMS.API.Common.Attributes;
using IPMS.API.Common.Enums;
using IPMS.API.Common.Extensions;
using IPMS.API.Responses;
using IPMS.Business.Common.Enums;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.ProjectSubmission;
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
        public async Task<IActionResult> UpdateProjectSubmissionName([FromBody] UpdateProjectSubmissionRequest request)
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

        [EnumAuthorize(UserRole.Lecturer)]
        [HttpPut("grade")]
        public async Task<IActionResult> GradeSubmission([FromBody] GradeSubmissionRequest request)
        {
            Guid currentUserId = HttpContext.User.Claims.GetUserId();
            await _projectSubmissionService.GradeSubmission(request, currentUserId);

            
            return GetActionResponse(new IPMSResponse<object>());
        }


        /// <summary>
        /// Get all submission in project of current user
        /// https://docs.google.com/spreadsheets/d/10eDAKGeT4Na1yPKZc3QiA6lA6ll7YHxKLWjGjOtLTNY/edit#gid=0
        /// </summary>
        [EnumAuthorize(UserRole.Student)]
        [HttpGet]
        public async Task<IActionResult> GetAllSubmission([FromQuery] GetAllSubmissionRequest request)
        {
            Guid currentUserId = HttpContext.User.Claims.GetUserId();
            var data = await _projectSubmissionService.GetAllSubmission(request, currentUserId);
            var response = await data.GetPaginatedResponse(page: request.Page, pageSize: request.PageSize);
            return GetActionResponse(response);
        }
    }
}
