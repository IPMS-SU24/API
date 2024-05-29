using IPMS.API.Common.Extensions;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Topic;
using Microsoft.AspNetCore.Mvc;

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
            var response = await _topicService.GetTopics(request).GetPaginatedResponse(page: request.Page, pageSize: request.PageSize);
            return Ok(response);
        }
    }
}
