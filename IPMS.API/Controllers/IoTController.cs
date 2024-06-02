using IPMS.API.Common.Extensions;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests;
using Microsoft.AspNetCore.Mvc;

using IPMS.API.Responses;
using IPMS.API.Common.Attributes;
using IPMS.API.Common.Enums;

namespace IPMS.API.Controllers
{
    public class IoTController : ApiControllerBase
    {
        private readonly IBorrowIoTService _borrowIoTService;
        public IoTController(IBorrowIoTService borrowIoTService)
        {
            _borrowIoTService = borrowIoTService;
        }
        [EnumAuthorize(UserRole.Leader)]
        [HttpPost("borrow")]
        public async Task<IActionResult> RegisterIoT([FromBody] List<BorrowIoTModelRequest> requests)
        {
            var leaderId = HttpContext.User.Claims.GetUserId();
            await _borrowIoTService.RegisterIoTForProject(leaderId,requests);
            return GetActionResponse(new IPMSResponse<object>());
        }
    }
}
