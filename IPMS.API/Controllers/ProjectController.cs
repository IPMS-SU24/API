using IPMS.API.Common.Attributes;
using IPMS.Business.Common.Enums;
using IPMS.API.Common.Extensions;
using IPMS.API.Responses;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Responses.ProjectDashboard;
using Microsoft.AspNetCore.Mvc;
using IPMS.API.Common.Enums;
using IPMS.Business.Requests.ProjectPreference;
using IPMS.Business.Responses.ProjectPreference;
using IPMS.DataAccess.Models;
using IPMS.Business.Requests.Project;

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

        /// <summary>
        /// Get Project Preferences
        /// https://docs.google.com/spreadsheets/d/1DalhYS3NT9XpwzBuaYKZrFrGB5L8H5cQ5mDva8T1ito/edit?gid=0#gid=0
        /// </summary>
        [EnumAuthorize(UserRole.Student)]
        [HttpGet("preferences")]
        public async Task<IActionResult> GetProjectPreferences([FromQuery] ProjectPreferenceRequest request)
        {
            var projectPreferences = await _projectService.GetProjectPreferences(request);
            var response = await projectPreferences.GetPaginatedResponse(page: request.Page, pageSize: request.PageSize);
            return Ok(response);
            
        }

        /// <summary>
        /// Get Groups Overview
        /// https://docs.google.com/spreadsheets/d/1t42RvlPCnbrJbrkG5fl_qfXpaK0XuR3loOi-S3R9Fok/edit?gid=0#gid=0
        /// </summary>
        [EnumAuthorize(UserRole.Lecturer)]
        [HttpGet("groups-overview")]
        public async Task<IActionResult> GetProjectsOverview([FromQuery] GetProjectsOverviewRequest request)
        {
            Guid currentUserId = HttpContext.User.Claims.GetUserId();
            var projectPreferences = await _projectService.GetProjectsOverview(request, currentUserId);
            var response = await projectPreferences.GetPaginatedResponse(page: request.Page, pageSize: request.PageSize);
            return Ok(response);

        }

        /// <summary>
        /// Get Group Detail
        /// https://docs.google.com/spreadsheets/d/1GY5tV2-HfVOyjpk551m1cZl5MegS31kw9PA7ZBumv-s/edit?gid=0#gid=0
        /// </summary>
        [EnumAuthorize(UserRole.Lecturer)]
        [HttpGet("group-detail")]
        public async Task<IActionResult> GetProjectDetail([FromQuery] GetProjectDetailRequest request)
        {
            Guid currentUserId = HttpContext.User.Claims.GetUserId();
            var projectPreferences = await _projectService.GetProjectDetail(request, currentUserId);
         //   var response = await projectPreferences.GetPaginatedResponse(page: request.Page, pageSize: request.PageSize);
            return Ok(projectPreferences);

        }
    }
}
