﻿using IPMS.API.Common.Attributes;
using IPMS.API.Common.Extensions;
using IPMS.Business.Common.Enums;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Class;
using Microsoft.AspNetCore.Mvc;
using IPMS.API.Responses;
using Microsoft.AspNetCore.Authorization;
using IPMS.Business.Responses.Class;
using System.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
using IPMS.DataAccess.Models;

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
        [Authorize]
        [HttpGet("{classId}/[action]")]
        public async Task<IActionResult> GroupsAsync(Guid classId)
        {
            var response = new IPMSResponse<IList<ClassGroupResponse>>()
            {
                Data = await _classService.GetGroupsInClass(classId)
            };
            return GetActionResponse(response);
        }
        [EnumAuthorize(UserRole.Lecturer)]
        [HttpPost("[action]")]
        public async Task<IActionResult> MembersInGroup([FromBody] MemberInGroupRequest request)
        {
            var resultQuery = await _classService.GetMemberInGroupAsync(request);
            var memberInfoResponse = await resultQuery.MemberInfo.GetPaginatedResponse(page: request.Page, pageSize: request.PageSize);
            var response = new PaginationResponse<object>()
            {
                PageSize = memberInfoResponse.PageSize,
                CurrentPage = memberInfoResponse.CurrentPage,
                TotalPage = memberInfoResponse.TotalPage,
                Data = new
                {
                    resultQuery.TotalMember,
                    MemberInfo = memberInfoResponse.Data
                }
            };
            return GetActionResponse(response);   
        }
        [EnumAuthorize(UserRole.Lecturer)]
        [HttpPost("[action]")]
        public async Task<IActionResult> AddStudents([FromBody] AddStudentsToClassRequest request)
        {
            await _classService.AddStudentAsync(request);
            return GetActionResponse(new IPMSResponse<object>());   
        }
        [EnumAuthorize(UserRole.Lecturer)]
        [HttpGet("{classId}/[action]")]
        public async Task<IActionResult> ImportStudentStatus(Guid classId)
        {
            var states = await _classService.GetImportStudentStatusAsync(classId);
            dynamic dataResponse = states != null ? states : "Processing";
            var response = new IPMSResponse<dynamic>()
            {
                Data = dataResponse
            };
            return GetActionResponse(response);   
        }

      //  [EnumAuthorize(UserRole.Admin)]
        [HttpGet("{classId}/[action]")]
        public async Task<IActionResult> GetClassDetail(Guid classId)
        {
            var detail = await _classService.GetClassDetail(classId);
            IPMSClass dataResponse = detail != null ? detail : new IPMSClass();
            var response = new IPMSResponse<IPMSClass>()
            {
                Data = dataResponse
            };
            return GetActionResponse(response);
        }
    }
}
