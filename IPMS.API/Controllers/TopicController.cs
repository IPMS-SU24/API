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
            var responseData =  _topicService.GetSuggestedTopics();
            var response = new IPMSResponse<IQueryable<Topic>> { Data = responseData };
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
