using IPMS.API.Common.Extensions;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.ClassTopic;
using IPMS.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IPMS.API.Controllers
{
    public class ClassTopicController : ApiControllerBase
    {
        private readonly IClassTopicService _classTopicService;

        public ClassTopicController(IClassTopicService classTopicService)
        {
            _classTopicService = classTopicService;
        }


        /// <summary>
        /// Specifies ClassTopics in class that haven't chosen yet. On other hand, they are available topic that can choose.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetClassTopics([FromQuery] GetClassTopicRequest request)
        {
            Guid currentUserId = new Guid(HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value);
            var classTopics = await _classTopicService.GetClassTopicsAvailable(currentUserId, request);
            var resposne = await classTopics.GetPaginatedResponse(page: request.Page, pageSize: request.PageSize);
            return Ok(resposne);
        }
    }
}
