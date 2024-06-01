using IPMS.API.Common.Enums;
using IPMS.API.Responses;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Models;
using IPMS.Business.Requests;
using IPMS.Business.Requests.Authentication;
using IPMS.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
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
    }
}
