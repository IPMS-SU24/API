using IPMS.API.Common.Attributes;
using IPMS.API.Common.Enums;
using IPMS.API.Common.Extensions;
using IPMS.API.Responses;
using IPMS.Business.Common.Enums;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.IoTComponent;
using IPMS.Business.Responses.ProjectDashboard;
using Microsoft.AspNetCore.Mvc;

namespace IPMS.API.Controllers
{
    [Route("api/v1/iot")]
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
        public async Task<IActionResult> RegisterIoT([FromBody] List<IoTModelRequest> requests)
        {
            var leaderId = HttpContext.User.Claims.GetUserId();
            await _borrowIoTService.RegisterIoTForProject(leaderId, requests);
            return GetActionResponse(new IPMSResponse<object>());
        }
        [EnumAuthorize(UserRole.Student)]
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableComponents([FromQuery] GetAvailableComponentRequest request)
        {
            var leaderId = HttpContext.User.Claims.GetUserId();
            var availableComponents = await _borrowIoTService.GetAvailableIoTComponents(request, leaderId);
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

        [EnumAuthorize(UserRole.Student)]
        [HttpGet("report")]
        public async Task<IActionResult> GetReportIoTComponents()
        {
            var data = await _borrowIoTService.GetGetReportIoTComponents();
            var response = new IPMSResponse<IEnumerable<ReportIoTComponentInformation>>()
            {
                Data = data
            };
            if (data == null)
            {
                response.Status = ResponseStatus.BadRequest;
            }
            return GetActionResponse(response);
        }


        [EnumAuthorize(UserRole.Lecturer)]
        [HttpPost("review-iots-borrow")]
        public async Task<IActionResult> GetBorrowIoTComponents([FromBody] GetBorrowIoTComponentsRequest request)
        {
            var currentUserId = HttpContext.User.Claims.GetUserId();
            var data = await _borrowIoTService.GetBorrowIoTComponents(request, currentUserId);
            var response = await data.GetPaginatedResponse(page: request.Page, pageSize: request.PageSize);
            return GetActionResponse(response);
        }

        [EnumAuthorize(UserRole.Lecturer)]
        [HttpPost("review-iots")]
        public async Task<IActionResult> ReviewBorrowIoTComponents([FromBody] ReviewBorrowIoTComponentsRequest request)
        {
            var currentUserId = HttpContext.User.Claims.GetUserId();
            await _borrowIoTService.ReviewBorrowIoTComponents(request, currentUserId);
            return GetActionResponse(new IPMSResponse<object>());

        }

        [EnumAuthorize(UserRole.Lecturer)]
        [HttpGet("repository")]
        public async Task<IActionResult> GetIoTRepositoryAsync([FromQuery] GetIoTRepositoryRequest request)
        {
            var lecturerId = HttpContext.User.Claims.GetUserId();
            var response = await _IoTDataService.GetIoTRepsitoryAsync(request, lecturerId);
            var paginationResponse = await response.info.GetPaginatedResponse(page: request.Page, pageSize: request.PageSize);
            var iotResponse = new IoTRepositoryResponse
            {
                PageSize = paginationResponse.PageSize,
                CurrentPage = paginationResponse.CurrentPage,
                Data = paginationResponse.Data,
                Errors = paginationResponse.Errors,
                Message = paginationResponse.Message,
                Status = paginationResponse.Status,
                TotalPage = paginationResponse.TotalPage,
                TotalComponents = response.TotalComponents
            };
            return GetActionResponse(paginationResponse);
        }

        [EnumAuthorize(UserRole.Lecturer)]
        [HttpPut("quantity")]
        public async Task<IActionResult> UpdateQuantity(UpdateIoTQuantityRequest request)
        {
            var lecturerId = HttpContext.User.Claims.GetUserId();
            await _IoTDataService.UpdateIoTQuantity(request, lecturerId);
            return GetActionResponse(new IPMSResponse<object>());
        }


       
    }
}
