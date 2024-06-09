using IPMS.API.Common.Attributes;
using IPMS.API.Common.Enums;
using IPMS.API.Common.Extensions;
using IPMS.API.Responses;
using IPMS.Business.Common.Enums;
using IPMS.Business.Interfaces.Services;
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
        public async Task<IActionResult> GetAssessmentById(Guid Id)
        {
           Guid currentUserId = HttpContext.User.Claims.GetUserId();

            var response = new IPMSResponse<AssessmentSubmissionProjectResponse>
            {
                Data = await _assessmentService.GetAssessmentById(Id, currentUserId)
                
            };
            return GetActionResponse(response);
        }

       
    }
}
