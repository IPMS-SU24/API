﻿using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Authentication;
using Microsoft.AspNetCore.Mvc;
using IPMS.API.Responses;
using IPMS.Business.Responses.Authentication;
using IPMS.Business.Requests.Admin;
using IPMS.API.Common.Extensions;
using IPMS.API.Common.Attributes;
using IPMS.Business.Common.Enums;
using IPMS.Business.Responses.Admin;
using IPMS.DataAccess.Models;

namespace IPMS.API.Controllers
{
    //[EnumAuthorize(UserRole.Admin)]
    public class AdminController : ApiControllerBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IAdminService _adminService;

        public AdminController(IAuthenticationService authenticationService, IAdminService adminService)
        {
            _authenticationService = authenticationService;
            _adminService = adminService;
        }

        [HttpPost("lecturer")]
        public async Task<IActionResult> AddLecturerAccount([FromBody] AddLecturerAccountRequest request)
        {
            await _authenticationService.AddLecturerAccount(request);

            return GetActionResponse(new IPMSResponse<object>());

        }
        [HttpPut("lecturer-update")]
        public async Task<IActionResult> UpdateLecturerAccount([FromBody] UpdateLecturerAccountRequest request)
        {
            await _authenticationService.UpdateLecturerAccount(request);

            return GetActionResponse(new IPMSResponse<object>());

        }
        [HttpGet("lecturer")]
        public async Task<IActionResult> GetAllLecturer()
        {
            var response = new IPMSResponse<IList<LectureAccountResponse>>()
            {
                Data = await _adminService.GetLecturerAsync()
            };
            return GetActionResponse(response);
        }

        [EnumAuthorize(UserRole.Admin)]
        [HttpGet("lecturer-list")]
        public async Task<IActionResult> GetLecturerList([FromQuery] GetLecturerListRequest request)
        {

            var lecturers = await _adminService.GetLecturerList(request);
            var response = await lecturers.GetPaginatedResponse(page: request.Page, pageSize: request.PageSize);

            return GetActionResponse(response);
        }

        [EnumAuthorize(UserRole.Admin)]
        [HttpGet("lecturer-detail")]
        public async Task<IActionResult> GetLecturerDetail([FromQuery] Guid lecturerId)
        {

            var lecturers = await _adminService.GetLecturerDetail(lecturerId);
            GetLecturerDetailResponse dataResponse = lecturers != null ? lecturers : new GetLecturerDetailResponse();
            var response = new IPMSResponse<GetLecturerDetailResponse>()
            {
                Data = dataResponse
            };
            return GetActionResponse(response);
        }

        [EnumAuthorize(UserRole.Admin)]
        [HttpGet("student-list")]
        public async Task<IActionResult> GetAllStudent([FromQuery] GetAllStudentRequest request)
        {

            var students = await _adminService.GetAllStudent(request);
            var response = await students.GetPaginatedResponse(page: request.Page, pageSize: request.PageSize);

            return GetActionResponse(response);
        }

        [EnumAuthorize(UserRole.Admin)]
        [HttpGet("student-detail")]
        public async Task<IActionResult> GetStudentDetail([FromQuery] Guid studentId)
        {

            var student = await _adminService.GetStudentDetail(studentId);

            var response = new IPMSResponse<GetStudentDetailResponse>()
            {
                Data = student
            };
            return GetActionResponse(response);
        }
        [EnumAuthorize(UserRole.Admin)]
        [HttpGet("report-list")]
        public async Task<IActionResult> GetReportList([FromQuery] GetReportListRequest request)
        {

            var reports = await _adminService.GetReportList(request);

            var response = await reports.GetPaginatedResponse(page: request.Page, pageSize: request.PageSize);

            return GetActionResponse(response);
        }

        [EnumAuthorize(UserRole.Admin)]
        [HttpGet("report-detail")]
        public async Task<IActionResult> GetReportDetail([FromQuery] Guid reportId)
        {

            var report = await _adminService.GetReportDetail(reportId);

            var response = new IPMSResponse<GetReportDetailResponse>()
            {
                Data = report
            };
            return GetActionResponse(response);

        }

        [EnumAuthorize(UserRole.Admin)]
        [HttpPut("report-update")]
        public async Task<IActionResult> ResponseReport([FromBody] ResponseReportRequest request)
        {
            await _adminService.ResponseReport(request);

            return GetActionResponse(new IPMSResponse<object>());

        }

        [EnumAuthorize(UserRole.Admin)]
        [HttpGet("assessment-detail")]
        public async Task<IActionResult> GetAssessmentDetail([FromQuery] Guid assessmentId)
        {

            var assessment = await _adminService.GetAssessmentDetail(assessmentId);
            var response = new IPMSResponse<GetAssessmentDetailResponse>()
            {
                Data = assessment
            };
            return GetActionResponse(response);

        }

        [EnumAuthorize(UserRole.Admin)]
        [HttpGet("syllabus-list")]
        public async Task<IActionResult> GetAllSyllabus([FromQuery] GetAllSyllabusRequest request)
        {

            var assessments = await _adminService.GetAllSyllabus(request);
            var response = await assessments.GetPaginatedResponse(page: request.Page, pageSize: request.PageSize);

            return GetActionResponse(response);
        }

        [EnumAuthorize(UserRole.Admin)]
        [HttpGet("syllabus-detail")]
        public async Task<IActionResult> GetSyllabusDetail([FromQuery] Guid syllabusId)
        {

            var syllabus = await _adminService.GetSyllabusDetail(syllabusId);
            var response = new IPMSResponse<GetSyllabusDetailResponse>()
            {
                Data = syllabus
            };
            return GetActionResponse(response);
        }

        [EnumAuthorize(UserRole.Admin)]
        [HttpPut("syllabus-update")]
        public async Task<IActionResult> UpdateSyllabus([FromBody] UpdateSyllabusRequest request)
        {
            await _adminService.UpdateSyllabus(request);
            return GetActionResponse(new IPMSResponse<object>());
        }

        [EnumAuthorize(UserRole.Admin)]
        [HttpPost("syllabus-clone")]
        public async Task<IActionResult> CloneSyllabus([FromBody] Guid syllabusId)
        {
            await _adminService.CloneSyllabus(syllabusId);
            return GetActionResponse(new IPMSResponse<object>());
        }
    }
}
