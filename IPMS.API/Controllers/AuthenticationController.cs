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
        private readonly UserManager<IPMSUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        public AuthenticationController(IAuthenticationService authenticationService, UserManager<IPMSUser> userManager, RoleManager<IdentityRole<Guid>> roleManager)
        {
            _authenticationService = authenticationService;
            _userManager = userManager;
            _roleManager = roleManager;

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
            if (userResult != null)
            {
                response.Status = ResponseStatus.Success;
            }
            else
            {
                response.Status = ResponseStatus.Unauthorized;
            }
            return GetResponse(response);
        }
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] Register request)
        {
            var user = new IPMSUser()
            {
                Email = request.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = request.Username,
                PhoneNumber = request.PhoneNumber,
                FullName = request.Fullname
            };
            var rs = await _userManager.CreateAsync(user, request.Password);
            if (rs.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync(UserRole.LECTURER.ToString())) await _roleManager.CreateAsync(new IdentityRole<Guid>(UserRole.LECTURER.ToString()));
                await _userManager.AddToRoleAsync(user, UserRole.LECTURER.ToString());
            }
            var response = new IPMSResponse<TokenModel>();
            return GetResponse(response);
        }
    }
}
