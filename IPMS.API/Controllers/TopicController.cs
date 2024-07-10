using IPMS.API.Common.Attributes;
using IPMS.API.Common.Enums;
using IPMS.API.Common.Extensions;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Topic;
using IPMS.API.Responses;
using Microsoft.AspNetCore.Mvc;
using IPMS.Business.Common.Enums;
using IPMS.Business.Responses.Report;
using IPMS.DataAccess.Models;
using IPMS.Business.Responses.Topic;

namespace IPMS.API.Controllers
{
    public class TopicController : ApiControllerBase
    {
        private readonly ITopicService _topicService;
        public TopicController(ITopicService topicService)
        {
            _topicService = topicService;
        }
        [HttpGet]
        public async Task<IActionResult> GetTopics([FromQuery] GetTopicRequest request)
        {
            var response = await _topicService.GetApprovedTopics(request).GetPaginatedResponse(page: request.Page, pageSize: request.PageSize);
            return GetActionResponse(response);
        }

        [HttpGet("suggested")]
        public async Task<IActionResult> GetSuggestedTopics()
        {
            var responseData = await _topicService.GetSuggestedTopics();
            var response = new IPMSResponse<IEnumerable<SuggestedTopicsResponse>> { Data = responseData };
            return GetActionResponse(response);
        }

        [HttpGet("lecturer-suggested")]
        public async Task<IActionResult> GetSuggestedTopicsLecturer([FromQuery] GetSuggestedTopicsLecturerRequest request)
        {
            Guid lecturerId = HttpContext.User.Claims.GetUserId();
            var suggested = await _topicService.GetSuggestedTopicsLecturer(request, lecturerId);
            var response = await suggested.GetPaginatedResponse(page: request.Page, pageSize: request.PageSize);
            return Ok(response);
        }
        [HttpGet("lecturer-suggested-detail")]
        public async Task<IActionResult> GetSuggestedTopicDetailLecturer([FromQuery] GetSugTopicDetailLecRequest request)
        {
            Guid lecturerId = HttpContext.User.Claims.GetUserId();
            var responseData = await _topicService.GetSuggestedTopicDetailLecturer(request, lecturerId);
            var response = new IPMSResponse<SuggestedTopicsResponse> { Data = responseData };
            return GetActionResponse(response);
        }
        [EnumAuthorize(UserRole.Leader)]
        [HttpPost("registration")]
        public async Task<IActionResult> RegisterNewTopic([FromBody] RegisterTopicRequest request)
        {
            var leaderId = User.Claims.GetUserId();
            await _topicService.RegisterTopic(request, leaderId);
            return GetActionResponse(new IPMSResponse<object>());
        }

        [EnumAuthorize(UserRole.Lecturer)]
        [HttpPost("lecturer-registration")]
        public async Task<IActionResult> LecturerRegisterNewTopic([FromBody] LecturerRegisterTopicRequest request)
        {
            var leaderId = User.Claims.GetUserId();
            await _topicService.LecturerRegisterNewTopic(request, leaderId);
            return GetActionResponse(new IPMSResponse<object>());
        }
    }
}
