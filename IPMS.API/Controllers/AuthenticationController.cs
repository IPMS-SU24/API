using IPMS.API.Common.Enums;
using IPMS.API.Common.Extensions;
using IPMS.API.Responses;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Models;
using IPMS.Business.Requests.Authentication;
using Microsoft.AspNetCore.Authorization;
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
            HttpContext.Session.Clear();
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
            HttpContext.Session.Clear();
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

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordRequest request)
        {
            await _authenticationService.ForgotPasswordAsync(request);
            return GetActionResponse(new IPMSResponse<object>()); ;
        }

        [HttpPut("reset-password")]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequest request)
        {
            await _authenticationService.ResetPasswordAsync(request);
            return GetActionResponse(new IPMSResponse<object>());
        }
        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePasswordAsync([FromBody] ChangePasswordRequest request)
        {
            var userId = HttpContext.User.Claims.GetUserId();
            await _authenticationService.ChangePasswordAsync(request, userId);
            return GetActionResponse(new IPMSResponse<object>());
        }
    }
}
