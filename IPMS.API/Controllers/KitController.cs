using IPMS.API.Common.Attributes;
using IPMS.API.Common.Extensions;
using IPMS.API.Responses;
using IPMS.Business.Common.Enums;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Kit;
using IPMS.Business.Responses.Admin;
using IPMS.Business.Responses.Kit;
using Microsoft.AspNetCore.Mvc;

namespace IPMS.API.Controllers
{
    [Route("api/v1/kit")]
    public class KitController : ApiControllerBase
    {
        private readonly IKitService _kitService;

        public KitController(IKitService kitService) 
        {
            _kitService = kitService;
        }

        [EnumAuthorize(UserRole.Admin)]
        [HttpPost("kit-get-all")]
        public async Task<IActionResult> GetAllKit([FromQuery] GetAllKitRequest request)
        {
            var kits = await _kitService.GetAllKit(request);
            var response = await kits.GetPaginatedResponse(page: request.Page, pageSize: request.PageSize);
            return GetActionResponse(response);
        }

        [EnumAuthorize(UserRole.Admin)]
        [HttpPost("kit-detail")]
        public async Task<IActionResult> GetKitDetail([FromQuery] Guid Id)
        {
            var kit = await _kitService.GetKitDetail(Id);
            var response = new IPMSResponse<KitResponse>()
            {
                Data = kit
            };
            return GetActionResponse(response);
        }

        [EnumAuthorize(UserRole.Admin)]
        [HttpPost("kit-create")]
        public async Task<IActionResult> CreateKit([FromBody] CreateKitRequest request)
        {
            await _kitService.CreateKit(request);
            return GetActionResponse(new IPMSResponse<object>());
        }
        [EnumAuthorize(UserRole.Admin)]
        [HttpPost("kit-update")]
        public async Task<IActionResult> UpdateKit([FromBody] UpdateKitRequest request)
        {
            await _kitService.UpdateKit(request);
            return GetActionResponse(new IPMSResponse<object>());
        }

        [EnumAuthorize(UserRole.Admin)]
        [HttpPost("basic-device-create")]
        public async Task<IActionResult> CreateBasicIoTDevice([FromBody] CreateBasicIoTDeviceRequest request)
        {
            await _kitService.CreateBasicIoTDevice(request);
            return GetActionResponse(new IPMSResponse<object>());
        }
        [EnumAuthorize(UserRole.Admin)]
        [HttpPost("basic-device-update")]
        public async Task<IActionResult> UpdateBasicIoTDevice([FromBody] UpdateBasicIoTDeviceRequest request)
        {
            await _kitService.UpdateBasicIoTDevice(request);
            return GetActionResponse(new IPMSResponse<object>());
        }
        [EnumAuthorize(UserRole.Admin)]
        [HttpPost("basic-device-delete")]
        public async Task<IActionResult> DeleteBasicIoTDevice([FromBody] Guid Id)
        {
            await _kitService.DeleteBasicIoTDevice(Id);
            return GetActionResponse(new IPMSResponse<object>());
        }
        [EnumAuthorize(UserRole.Admin)]
        [HttpPost("basic-device-get-all")]
        public async Task<IActionResult> GetAllBasicIoTDevice([FromQuery] GetAllBasicIoTDeviceRequest request)
        {
            var basicDevices = await _kitService.GetAllBasicIoTDevice(request);
            var response = await basicDevices.GetPaginatedResponse(page: request.Page, pageSize: request.PageSize);
            return GetActionResponse(response);
        }

        [EnumAuthorize(UserRole.Admin)]
        [HttpPost("basic-device-detail")]
        public async Task<IActionResult> GetBasicDetail([FromQuery] Guid Id)
        {
            var basicIot = await _kitService.GetBasicDetail(Id);
            var response = new IPMSResponse<BasicIoTDeviceResponse>()
            {
                Data = basicIot
            };
            return GetActionResponse(response);
        }

    }
}
