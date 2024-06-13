using IPMS.API.Common.Attributes;
using IPMS.API.Common.Extensions;
using IPMS.Business.Common.Enums;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.MemberHistory;
using Microsoft.AspNetCore.Mvc;

namespace IPMS.API.Controllers
{
    public class MemberHistoryController : ApiControllerBase
    {
        private readonly IMemberHistoryService _memberHistoryService;
        public MemberHistoryController(IMemberHistoryService MemberHistoryService)
        {
            _memberHistoryService = MemberHistoryService;
        }

        [EnumAuthorize(UserRole.Student)]
        [HttpGet]
        public async Task<IActionResult> GetLoggedInUserHistory([FromQuery] GetLoggedInUserHistoryRequest request)
        {
            Guid currentUserId = HttpContext.User.Claims.GetUserId();
            var data = await _memberHistoryService.GetLoggedInUserHistories(currentUserId);
            var response = await data.GetPaginatedResponse(page: request.Page, pageSize: request.PageSize);
            return GetActionResponse(response);
        }

        [EnumAuthorize(UserRole.Student, UserRole.Leader)]
        [HttpPut]
        public async Task<IActionResult> UpdateRequestStatus([FromBody] UpdateRequestStatusRequest request)
        {
            Guid currentUserId = HttpContext.User.Claims.GetUserId();

            await _memberHistoryService.UpdateRequestStatus(request, currentUserId);
            return Ok();
        }
    }
}
