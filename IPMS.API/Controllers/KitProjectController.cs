using IPMS.API.Common.Attributes;
using IPMS.API.Common.Extensions;
using IPMS.API.Responses;
using IPMS.Business.Common.Enums;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Kit;
using IPMS.Business.Requests.KitProject;
using IPMS.Business.Requests.ProjectKit;
using IPMS.Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace IPMS.API.Controllers
{
    [Route("api/v1/kit-project")]
    public class KitProjectController : ApiControllerBase
    {
        private readonly IKitProjectService _kitProjectService;

        public KitProjectController(IKitProjectService kitProjectService)
        {
            _kitProjectService = kitProjectService;
        }
        [EnumAuthorize(UserRole.Admin)]
        [HttpPost("get-all")]
        public async Task<IActionResult> GetAllKit([FromQuery] GetAllKitProjectRequest request)
        {
            var kitsProject = await _kitProjectService.GetAllKitProject(request);
            var response = await kitsProject.GetPaginatedResponse(page: request.Page, pageSize: request.PageSize);
            return GetActionResponse(response);
        }

        [EnumAuthorize(UserRole.Admin)]
        [HttpPost("create")]
        public async Task<IActionResult> CreateKitProject([FromBody] CreateKitProjectRequest request)
        {
            await _kitProjectService.CreateKitProject(request);
            return GetActionResponse(new IPMSResponse<object>());
        }

        [EnumAuthorize(UserRole.Admin)]
        [HttpPost("return")]
        public async Task<IActionResult> ReturnKitProject([FromBody] UpdateKitProjectRequest request)
        {
            await _kitProjectService.UpdateKitProject(request);
            return GetActionResponse(new IPMSResponse<object>());
        }

    }
}
