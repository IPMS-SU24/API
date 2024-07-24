using IPMS.API.Common.Attributes;
using IPMS.API.Common.Enums;
using IPMS.API.Common.Extensions;
using IPMS.API.Responses;
using IPMS.Business.Common.Enums;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.ClassTopic;
using IPMS.Business.Requests.Topic;
using IPMS.Business.Responses.Topic;
using Microsoft.AspNetCore.Mvc;
using static IPMS.API.Common.Extensions.UserExtensions;

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
        /// https://docs.google.com/spreadsheets/d/1m8HHx6pYmBw-fNgilKLwJJ7M9m9AGki4it0be34URwQ/edit#gid=0
        /// </summary>
        [EnumAuthorize(UserRole.Student)]
        [HttpGet]
        public async Task<IActionResult> GetClassTopics([FromQuery] GetClassTopicRequest request)
        {
            Guid currentUserId = HttpContext.User.Claims.GetUserId();
            var classTopics = await _classTopicService.GetClassTopicsAvailable(currentUserId, request);
            var response = await classTopics.GetPaginatedResponse(page: request.Page, pageSize: request.PageSize);
            return GetActionResponse(response);
        }

        [EnumAuthorize(UserRole.Lecturer)]
        [HttpGet("[action]")]
        public async Task<IActionResult> ClassTopicsByLecturer([FromRoute] LecturerClassTopicRequest request)
        {
            Guid currentUserId = HttpContext.User.Claims.GetUserId();
            var classTopics = await _classTopicService.GetClassTopicsByLecturer(currentUserId, request);
            var response = new IPMSResponse<IList<LecturerTopicIotComponentReponse>>()
            {
                Data = classTopics
            };
            return GetActionResponse(response);
        }

        /// <summary>
        /// Pick ClassTopics in class that haven't chosen yet.
        /// Can change topic if in deadline pick topic
        /// https://docs.google.com/spreadsheets/d/1m8HHx6pYmBw-fNgilKLwJJ7M9m9AGki4it0be34URwQ/edit#gid=1001191212
        /// </summary>
        [EnumAuthorize(UserRole.Leader)]
        [HttpPut]
        public async Task<IActionResult> PickTopic([FromBody] PickTopicRequest request)
        {
            bool isPickedSuccessfully = await _classTopicService.PickTopic(request.TopicId);

            var response = new IPMSResponse<object>(); // Default is success

            if (!isPickedSuccessfully) {
                response.Status = ResponseStatus.BadRequest;
                response.Message = "Not Success";
            }

            return GetActionResponse(response);
        }
    }
}
