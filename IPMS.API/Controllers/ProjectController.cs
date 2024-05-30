using IPMS.API.Common.Enums;
using IPMS.API.Common.Extensions;
using IPMS.API.Responses;
using IPMS.Business.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace IPMS.API.Controllers
{
    public class ProjectController : ApiControllerBase
    {
        private readonly IProjectService _projectService;
        public ProjectController(IProjectService ProjectService)
        {
            _projectService = ProjectService;
        }
        [HttpGet]
        public async Task<IActionResult> GetProjectName()
        {
          //  Guid currentUserId = HttpContext.User.Claims.GetUserId();

            var projectName = await _projectService.GetProjectName(new Guid("9c7d4c9e-6a23-4c13-af6d-e85b83705b2e"));

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
    }
}
