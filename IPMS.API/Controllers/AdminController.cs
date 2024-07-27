using IPMS.API.Common.Attributes;
using IPMS.Business.Common.Enums;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Authentication;
using Microsoft.AspNetCore.Mvc;
using IPMS.API.Responses;
using IPMS.Business.Responses.Authentication;

namespace IPMS.API.Controllers
{
    //[EnumAuthorize(UserRole.Admin)]
    public class AdminController : ApiControllerBase
    {
        private readonly IAuthenticationService _authenticationService;

        public AdminController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpPost("lecturer")]
        public async Task<IActionResult> AddLecturerAccount([FromBody] AddLecturerAccountRequest request)
        {
            await _authenticationService.AddLecturerAccount(request);
            return GetActionResponse(new IPMSResponse<object>());
        }
        [HttpGet("lecturer")]
        public async Task<IActionResult> GetAllLecturer()
        {
            var response = new IPMSResponse<IList<LectureAccountResponse>>()
            {
                Data = await _authenticationService.GetLecturerAsync()
            };
            return GetActionResponse(response);
        }
    }
}
