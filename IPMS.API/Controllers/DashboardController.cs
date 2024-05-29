using IPMS.API.Filters;
using IPMS.API.Responses;
using IPMS.Business.Common.Singleton;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IPMS.API.Controllers
{
    public class DashboardController : ApiControllerBase
    {
        private readonly IProjectDashboardService _projectDashboardService;
        public DashboardController(IProjectDashboardService projectDashboardService)
        {
            _projectDashboardService = projectDashboardService;
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value);
            var response = new IPMSResponse<GetProjectDetailData>
            {
                Data = await _projectDashboardService.GetProjectDetail(userId)
            };
            return GetActionResponse(response);
        }
    }
}
