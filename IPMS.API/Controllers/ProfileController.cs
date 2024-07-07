using IPMS.API.Common.Attributes;
using IPMS.API.Common.Extensions;
using IPMS.Business.Common.Enums;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Responses.Profile;
using Microsoft.AspNetCore.Mvc;

namespace IPMS.API.Controllers
{
    public class ProfileController : ApiControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }
        [EnumAuthorize(UserRole.Student)]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var studentId = HttpContext.User.Claims.GetUserId();
            var response = await _profileService.GetProfile(studentId);
            return GetActionResponse(new Responses.IPMSResponse<ProfileResponse>
            {
                Data = response,
            });
        }
    }
}
