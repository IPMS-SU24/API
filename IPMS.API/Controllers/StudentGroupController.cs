using IPMS.API.Common.Attributes;
using IPMS.API.Common.Enums;
using IPMS.API.Common.Extensions;
using IPMS.API.Responses;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Responses.Group;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IPMS.API.Controllers
{
    /// <summary>
    /// https://docs.google.com/spreadsheets/d/1cJsUChJAg-zVutbhDCjYB-enlN-QiKRg1jbe78xoCKI/
    /// </summary>
    public class StudentGroupController : ApiControllerBase
    {
        private readonly IStudentGroupService _studentGroupService;
        public StudentGroupController(IStudentGroupService studentGroupService)
        {
            _studentGroupService = studentGroupService;
        }
        [EnumAuthorize(UserRole.Student)]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var studentId = User.Claims.GetUserId();
            var response = new IPMSResponse<StudentGroupResponse>
            {
                Data = await _studentGroupService.GetStudentGroupInformation(studentId)
            };
            return GetActionResponse(response);
        }
    }
}
