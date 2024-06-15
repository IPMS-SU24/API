using IPMS.API.Common.Attributes;
using IPMS.API.Common.Extensions;
using IPMS.API.Responses;
using IPMS.Business.Common.Enums;
using IPMS.Business.Interfaces.Services;
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
            var response = new IPMSResponse<IEnumerable<GetAllSemestersResponse>>
            {
                Data = semesters
            };
            return GetActionResponse(response);
        }
    }
}
