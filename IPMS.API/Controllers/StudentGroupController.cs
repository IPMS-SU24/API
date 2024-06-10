using FluentValidation;
using IPMS.API.Common.Attributes;
using IPMS.API.Common.Enums;
using IPMS.API.Common.Extensions;
using IPMS.API.Responses;
using IPMS.API.Validators.Group;
using IPMS.Business.Common.Enums;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Group;
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
        [EnumAuthorize(UserRole.Student)]
        [HttpPost]
        public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest request)

        {
            var studentId = User.Claims.GetUserId();
            await _studentGroupService.CreateGroup(request,studentId);
            return GetActionResponse(new IPMSResponse<object>());
        }
        [EnumAuthorize(UserRole.Student)]
        [HttpPost("swap")]
        public async Task<IActionResult> RequestToSwapGroup([FromBody] SwapGroupRequest request)
        {
            var studentId = User.Claims.GetUserId();
            await _studentGroupService.RequestToSwapGroup(request,studentId);
            return GetActionResponse(new IPMSResponse<object>());
        }
        [EnumAuthorize(UserRole.Student)]
        [HttpPost("join")]
        public async Task<IActionResult> RequestToJoinGroup([FromBody] JoinGroupRequest request)
        {
            var studentId = User.Claims.GetUserId();
            await _studentGroupService.RequestToJoinGroup(request,studentId);
            return GetActionResponse(new IPMSResponse<object>());
        }
    }
}
