using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Authentication;
using Microsoft.AspNetCore.Mvc;
using IPMS.API.Responses;
using IPMS.Business.Responses.Authentication;
using IPMS.Business.Requests.Admin;
using IPMS.API.Common.Extensions;
using IPMS.API.Common.Attributes;
using IPMS.Business.Common.Enums;
using IPMS.Business.Responses.Admin;

namespace IPMS.API.Controllers
{
    //[EnumAuthorize(UserRole.Admin)]
    public class AdminController : ApiControllerBase
    {
        private readonly IAuthenticationService _authenticationService;

        public AdminController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
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
                Data = await _authenticationService.GetLecturerAsync()
            };
            return GetActionResponse(response);
        }

        [EnumAuthorize(UserRole.Admin)]
        [HttpGet("lecturer-list")]
        public async Task<IActionResult> GetLecturerList([FromQuery] GetLecturerListRequest request)
        {

            var lecturers = await _authenticationService.GetLecturerList(request);
            var response = await lecturers.GetPaginatedResponse(page: request.Page, pageSize: request.PageSize);

            return GetActionResponse(response);
        }

        [EnumAuthorize(UserRole.Admin)]
        [HttpGet("lecturer-detail")]
        public async Task<IActionResult> GetLecturerDetail([FromQuery] Guid lecturerId)
        {

            var lecturers = await _authenticationService.GetLecturerDetail(lecturerId);
            GetLecturerDetailResponse dataResponse = lecturers != null ? lecturers : new GetLecturerDetailResponse();
            var response = new IPMSResponse<GetLecturerDetailResponse>()
            {
                Data = dataResponse
            };
            return GetActionResponse(response);
        }

        [EnumAuthorize(UserRole.Admin)]
        [HttpGet("student")]
        public async Task<IActionResult> GetAllStudent([FromQuery] GetAllStudentRequest request)
        {

            var students = await _authenticationService.GetAllStudent(request);
            var response = await students.GetPaginatedResponse(page: request.Page, pageSize: request.PageSize);

            return GetActionResponse(response);
        }

        [EnumAuthorize(UserRole.Admin)]
        [HttpGet("student-detail")]
        public async Task<IActionResult> GetStudentDetail([FromQuery] Guid studentId)
        {

            var student = await _authenticationService.GetStudentDetail(studentId);

            var response = new IPMSResponse<GetStudentDetailResponse>()
            {
                Data = student
            };
            return GetActionResponse(response);
        }
    }
}
