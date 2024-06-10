using IPMS.API.Common.Attributes;
using IPMS.Business.Common.Enums;
using IPMS.API.Common.Extensions;
using IPMS.API.Responses;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Responses.ProjectDashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IPMS.API.Common.Enums;

namespace IPMS.API.Controllers
{
    public class ProjectController : ApiControllerBase
    {
        private readonly IProjectService _projectService;
        public ProjectController(IProjectService ProjectService)
        {
            _projectService = ProjectService;
        }
        [EnumAuthorize(UserRole.Student)]
        [HttpGet("name")]
        public async Task<IActionResult> GetProjectName()
        {
            Guid currentUserId = HttpContext.User.Claims.GetUserId();

            var projectName = await _projectService.GetProjectName(currentUserId);

            //set default is this student is studying
            var response = new IPMSResponse<object>()
            {
                Data = projectName,
            }; // Default is success

            if (projectName == null)
            {
                response.Status = ResponseStatus.BadRequest;
                response.Message = "Not studying";
                response.Data = null;
            }

            return GetActionResponse(response);
        }
        [EnumAuthorize(UserRole.Student)]
        [HttpGet("detail")]  
        public async Task<IActionResult> GetProjectProgress()
        {
            Guid currentUserId = HttpContext.User.Claims.GetUserId();
            var projectProgress = await _projectService.GetProjectProgressData(currentUserId);
            var response = new IPMSResponse<ProjectProgressData>()
            {
                Data = projectProgress
            };
            if(projectProgress == null)
            {
                response.Status = ResponseStatus.BadRequest;
            }
            return GetActionResponse(response);
        }
    }
}
