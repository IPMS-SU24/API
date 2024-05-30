using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Project;
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
          //  var response = await _projectService.GetProjects(request).GetPaginatedResponse(page: request.Page, pageSize: request.PageSize);
            return Ok();
        }
    }
}
