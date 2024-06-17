using IPMS.API.Common.Attributes;
using IPMS.API.Common.Extensions;
using IPMS.Business.Common.Enums;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Class;
using Microsoft.AspNetCore.Mvc;
using IPMS.API.Responses;

namespace IPMS.API.Controllers
{
    public class ClassController : ApiControllerBase
    {
        private readonly IClassService _classService;
        public ClassController(IClassService classService)
        {
            _classService = classService;
        }
        [EnumAuthorize(UserRole.Lecturer)]
        [HttpPut("max-member")]
        public async Task<IActionResult> SetMaxMemberForClass([FromBody] SetMaxMemberRequest request)
        {
            var lecturerId = User.Claims.GetUserId();
            await _classService.SetMaxMember(lecturerId, request);
            return GetActionResponse(new IPMSResponse<object>());   
        }
    }
}
