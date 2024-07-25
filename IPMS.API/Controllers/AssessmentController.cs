using IPMS.API.Common.Attributes;
using IPMS.API.Common.Extensions;
using IPMS.API.Responses;
using IPMS.Business.Common.Enums;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Assessment;
using IPMS.Business.Responses.Assessment;
using Microsoft.AspNetCore.Mvc;

namespace IPMS.API.Controllers
{
    public class AssessmentController : ApiControllerBase
    {
        private readonly IAssessmentService _assessmentService;
        public AssessmentController(IAssessmentService AssessmentService)
        {
            _assessmentService = AssessmentService;
        }

        /// <summary>
        /// Get Assessment with Submission Module and Project Submission of User through AssessmentId
        /// https://docs.google.com/spreadsheets/d/1y7rOIEJtgFBYP80z8zMoRy_AWJ_VWKMFA-y3o7L83_M/edit#gid=0
        /// </summary>
        [EnumAuthorize(UserRole.Student)]
        [HttpGet]
        public async Task<IActionResult> GetAssessmentById([FromQuery] GetAssessmentByIdRequest request)
        {
            var response = new IPMSResponse<AssessmentSubmissionProjectResponse>
            {
                Data = await _assessmentService.GetAssessmentById(request.Id)
            };
            return GetActionResponse(response);
        }

        /// <summary>
        /// Get All Assessments
        /// https://docs.google.com/spreadsheets/d/1FLYnGkUGJMFVO-G5EsxWFwZ5rjQMVt92GUpUOn5JUFY/edit?gid=0#gid=0
        /// </summary>
        [EnumAuthorize(UserRole.Admin)]
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllAssessments([FromQuery] GetAllAssessmentsRequest request)
        {
            var assessments = await _assessmentService.GetAllAssessments(request);
            var response = await assessments.GetPaginatedResponse(page: request.Page, pageSize: request.PageSize);
            return GetActionResponse(response);
        }

    }
}
