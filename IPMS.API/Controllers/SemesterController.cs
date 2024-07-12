using IPMS.API.Common.Attributes;
using IPMS.API.Common.Extensions;
using IPMS.API.Responses;
using IPMS.Business.Common.Enums;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Semester;
using IPMS.Business.Responses.Semester;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IPMS.API.Controllers
{
    public class SemesterController : ApiControllerBase
    {
        private readonly ISemesterService _semesterService;
        public SemesterController(ISemesterService semesterService)
        {
            _semesterService = semesterService;
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllSemesters()
        {
            var semesters = await _semesterService.GetAllSemesters();
            var response = new IPMSResponse<GetAllSemestersResponse>
            {
                Data = semesters
            };
            return GetActionResponse(response);
        }
        [Authorize]
        [HttpGet("[action]")]
        public async Task<IActionResult> CurrentSemesterAsync()
        {
            var semester = await _semesterService.GetCurrentSemester();
            var response = new IPMSResponse<GetCurrentSemesterResponse>
            {
                Data = semester
            };
            return GetActionResponse(response);
        }
        [EnumAuthorize(UserRole.Lecturer)]
        [HttpGet("classes")]
        public async Task<IActionResult> GetClassesInSemester([FromQuery] GetClassInfoInSemesterRequest request)
        {
            var lecturerId = User.Claims.GetUserId();
            var classes = await _semesterService.GetClassesInSemester(lecturerId, request);
            var response = new IPMSResponse<GetClassInfoInSemesterResponse>
            {
                Data = classes
            };
            return GetActionResponse(response);
        }
    }
}
