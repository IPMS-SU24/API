﻿using IPMS.API.Common.Attributes;
using IPMS.API.Responses;
using IPMS.Business.Common.Enums;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Responses.ProjectDashboard;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static IPMS.API.Common.Extensions.UserExtensions;

namespace IPMS.API.Controllers
{
    public class ProjectDashboardController : ApiControllerBase
    {
        private readonly IProjectDashboardService _projectDashboardService;
        public ProjectDashboardController(IProjectDashboardService projectDashboardService)
        {
            _projectDashboardService = projectDashboardService;
        }
        [EnumAuthorize(UserRole.Student)]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userId = HttpContext.User.Claims.GetUserId();
            var response = new IPMSResponse<GetProjectDetailData>
            {
                Data = await _projectDashboardService.GetProjectDetail(userId)
            };
            return GetActionResponse(response);
        }
        [EnumAuthorize(UserRole.Student)]
        [HttpGet("near-deadlines")]
        public async Task<IActionResult> GetNearDealines()
        {
            var userId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value);
            var response = new IPMSResponse<NearSubmissionDeadlineData>
            {
                Data = await _projectDashboardService.GetNearSubmissionDeadlines(userId)
            };
            return GetActionResponse(response);
        }
    }
}
