using IPMS.API.Common.Enums;
using IPMS.API.Common.Extensions;
using IPMS.API.Responses;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Models;
using IPMS.Business.Requests.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IPMS.API.Controllers
{
    public class AuthenticationController : ApiControllerBase
    {
        private readonly IAuthenticationService _authenticationService;
        public AuthenticationController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var userResult = await _authenticationService.Login(request);
            var response = new IPMSResponse<TokenModel>
            {
                Data = userResult
        };
            if(userResult == null)
            {
                response.Status = ResponseStatus.Unauthorized;
            }

            return GetActionResponse(response);
        }
        [HttpPost]
        [Route("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenModel request)
        {
            var rs = await _authenticationService.RefreshToken(request);

            var response = new IPMSResponse<TokenModel>
            {
                Data = rs
            };
            if (rs is null)
            {
                response.Status = ResponseStatus.BadRequest;
                response.Message = "AccessToken or RefreshToken is not valid";
            }
            return GetActionResponse(response);
        }
        [HttpGet("email-confirmation")]
        public async Task<IActionResult> ConfirmEmail(Guid userId, string token)
        {
            await _authenticationService.ConfirmEmailAsync(userId, token);
            return GetActionResponse(new IPMSResponse<object>());
        }
    }
}
