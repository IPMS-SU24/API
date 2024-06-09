using IPMS.API.Common.Extensions;
using IPMS.Business.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

using IPMS.API.Responses;
using IPMS.API.Common.Attributes;
using IPMS.API.Common.Enums;
using IPMS.Business.Requests.IoTComponent;
using IPMS.Business.Responses.ProjectDashboard;

namespace IPMS.API.Controllers
{
    public class IoTController : ApiControllerBase
    {
        private readonly IBorrowIoTService _borrowIoTService;
        private readonly IIoTDataService _IoTDataService;
        public IoTController(IBorrowIoTService borrowIoTService, IIoTDataService ioTDataService)
        {
            _borrowIoTService = borrowIoTService;
            _IoTDataService = ioTDataService;
        }
        [EnumAuthorize(UserRole.Leader)]
        [HttpPost("borrow")]
        public async Task<IActionResult> RegisterIoT([FromBody] List<BorrowIoTModelRequest> requests)
        {
            var leaderId = HttpContext.User.Claims.GetUserId();
            await _borrowIoTService.RegisterIoTForProject(leaderId,requests);
            return GetActionResponse(new IPMSResponse<object>());
        }
        [EnumAuthorize(UserRole.Student)]
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableComponents([FromQuery] GetAvailableComponentRequest request )
        {
            var leaderId = HttpContext.User.Claims.GetUserId();
            var availableComponents = await _borrowIoTService.GetAvailableIoTComponents(request,leaderId);
            var response = new IPMSResponse<IEnumerable<BorrowIoTComponentInformation>>()
            {
                Data = availableComponents
            };
            if (availableComponents == null)
            {
                response.Status = ResponseStatus.BadRequest;
            }
            return GetActionResponse(response);
        }
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] GetIoTComponentRequest request)
        {
            var response = await _IoTDataService.GetAll(request).GetPaginatedResponse();
            return GetActionResponse(response);
        }
    }
}
