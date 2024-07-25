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
        public async Task<IActionResult> CreateGroup()
        {
            var studentId = User.Claims.GetUserId();
            var validator = new CreateGroupRequestValidator(_studentGroupService);
            var validateResult = await validator.ValidateAsync(new CreateGroupValidateRequest
            {
                StudentId = studentId
            });
            if (!validateResult.IsValid)
            {
                return new BadRequestObjectResult(new IPMSResponse<object>
                {
                    Status = ResponseStatus.BadRequest,
                    Errors = validateResult.ToDictionary()
                });
            }
            var response = await _studentGroupService.CreateGroup(studentId);
            return GetActionResponse(new IPMSResponse<CreateGroupResponse>
            {
                Data = response
            });
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
        [EnumAuthorize(UserRole.Leader, UserRole.Lecturer)]
        [HttpPut("leader-assignment")]
        public async Task<IActionResult> AssignLeader([FromBody] AssignLeaderRequest request)
        {
            var studentId = User.Claims.GetUserId();
            await _studentGroupService.AssignLeader(request,studentId);
            return GetActionResponse(new IPMSResponse<object>());
        }

        [EnumAuthorize(UserRole.Lecturer)]
        [HttpPut("remove-student-group")]
        public async Task<IActionResult> RemoveStudentOutGroup([FromBody] RemoveStudentOutGroupRequest request)
        {
            var lecturerId = User.Claims.GetUserId();
            await _studentGroupService.RemoveStudentOutGroup(request, lecturerId);
            return GetActionResponse(new IPMSResponse<object>());
        }

        [EnumAuthorize(UserRole.Lecturer)]
        [HttpPut("add-students")]
        public async Task<IActionResult> AddStudentsToGroup([FromBody] LecturerAddStudentsToGroupRequest request)
        {
            var lecturerId = User.Claims.GetUserId();
            await _studentGroupService.AddStudentsToGroup(request, lecturerId);
            return GetActionResponse(new IPMSResponse<object>());
        }

        [EnumAuthorize(UserRole.Leader)]
        [HttpPut("[action]")]
        public async Task<IActionResult> EvaluateMembers([FromBody] LeaderEvaluateMembersRequest request)
        {
            var leaderId = User.Claims.GetUserId();
            await _studentGroupService.EvaluateMembers(request, leaderId);
            return GetActionResponse(new IPMSResponse<object>());
        }

        [EnumAuthorize(UserRole.Student)]
        [HttpGet("student-evaluate-members")]
        public async Task<IActionResult> GetEvaluateMembers()
        {
            var studentId = User.Claims.GetUserId();
            var response = await _studentGroupService.GetEvaluateMembers(studentId);
            return GetActionResponse(new IPMSResponse<IList<MemberEvaluateResponse>>
            {
                Data = response
            });
        }

        [EnumAuthorize(UserRole.Lecturer)]
        [HttpGet("lecturer-evaluate-members/{groupId}")]
        public async Task<IActionResult> GetEvaluateMembersByLecturer(GetMemberContributionRequest request)
        {
            var response = await _studentGroupService.GetEvaluateMembersByLecturer(request);
            return GetActionResponse(new IPMSResponse<IList<MemberEvaluateResponse>>
            {
                Data = response
            });
        }
    }
}
