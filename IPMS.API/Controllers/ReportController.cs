using IPMS.API.Common.Attributes;
using IPMS.API.Common.Extensions;
using IPMS.API.Responses;
using IPMS.Business.Common.Enums;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Report;
using IPMS.Business.Responses.Report;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IPMS.API.Controllers
{
    public class ReportController : ApiControllerBase
    {
        private readonly IReportService _reportService;
        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }
        [Authorize]
        [HttpGet("types")]
        public async Task<IActionResult> GetReportType()
        {
            var responseData = await _reportService.GetReportType();
            var response = new IPMSResponse<IEnumerable<ReportTypeResponse>> { Data = responseData };
            return GetActionResponse(response);
        }
        [EnumAuthorize(UserRole.Student, UserRole.Lecturer)]
        [HttpPost]
        public async Task<IActionResult> SendReport(SendReportRequest request)
        {
            var reporterId = HttpContext.User.Claims.GetUserId();
            await _reportService.SendReport(request, reporterId);
            var response = new IPMSResponse<object>();
            return GetActionResponse(response);
        }
        [EnumAuthorize(UserRole.Student)]
        [HttpGet("student-report")]
        public async Task<IActionResult> GetStudentReport([FromQuery] StudentReportRequest request)
        {
            var reporterId = HttpContext.User.Claims.GetUserId();
            var responseData = await _reportService.GetStudentReport(request, reporterId);
            var response = new IPMSResponse<IEnumerable<StudentReportResponse>> { Data = responseData };
            return GetActionResponse(response);
        }
    }
}
