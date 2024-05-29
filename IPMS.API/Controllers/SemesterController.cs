
using IPMS.API.Common.Extensions;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Semester;
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
        [HttpGet]
        public async Task<IActionResult> GetSemesters([FromQuery] GetSemesterRequest request)
        {
            var response =  await _semesterService.GetSemesters(request).GetPaginatedResponse(page: request.Page, pageSize: request.PageSize);
            return Ok(response);
        }
    }
}
