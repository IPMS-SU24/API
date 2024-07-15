using IPMS.API.Common.Attributes;
using IPMS.API.Common.Extensions;
using IPMS.API.Responses;
using IPMS.Business.Common.Enums;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.FavoriteTopic;
using IPMS.Business.Responses.FavoriteTopic;
using Microsoft.AspNetCore.Mvc;

namespace IPMS.API.Controllers
{
    [EnumAuthorize(UserRole.Lecturer)]
    public class FavoriteTopicListController : ApiControllerBase
    {
        private readonly IFavoriteTopicListService _favoriteTopicListService;

        public FavoriteTopicListController(IFavoriteTopicListService favoriteTopicListService)
        {
            _favoriteTopicListService = favoriteTopicListService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var response = await _favoriteTopicListService.GetAsync(HttpContext.User.Claims.GetUserId());
            return GetActionResponse(new IPMSResponse<IList<GetAllFavoriteResponse>>
            {
                Data = response
            });
        }

        /***
         * Get list favorite and topics in list
         * https://docs.google.com/spreadsheets/d/1PLw2eOghlf4kPk_JU8qFFppeHfD0ap3LGIic7SPsBko/edit?gid=0#gid=0
         ***/
        [HttpGet("list-topics")]
        public async Task<IActionResult> GetListTopic()
        {
            var response = await _favoriteTopicListService.GetListTopic(HttpContext.User.Claims.GetUserId());
            return GetActionResponse(new IPMSResponse<IList<GetListTopicResponse>>
            {
                Data = response
            });
        }

        /***
         * Get list favorite and topics in list
         * https://docs.google.com/spreadsheets/d/1PLw2eOghlf4kPk_JU8qFFppeHfD0ap3LGIic7SPsBko/edit?gid=0#gid=0
         ***/
        [HttpPut("assign-topic")]
        public async Task<IActionResult> AssignTopicList(AssignTopicListRequest request)
        {
            await _favoriteTopicListService.AssignTopicList(request, HttpContext.User.Claims.GetUserId());
            return GetActionResponse(new IPMSResponse<object>());
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateFavoriteTopicListRequest request)
        {
            var response = new IPMSResponse<CreateFavoriteTopicListResponse>
            {
                Data = await _favoriteTopicListService.Create(request, HttpContext.User.Claims.GetUserId())
            };
            return GetActionResponse(response);
        }
        [HttpPut]
        public async Task<IActionResult> UpdateAsync(UpdateFavoriteTopicListRequest request)
        {
            await _favoriteTopicListService.UpdateAsync(request, HttpContext.User.Claims.GetUserId());
            return GetActionResponse(new IPMSResponse<object>());
        }
        [HttpDelete("{favoriteId}")]
        public async Task<IActionResult> DeleteAsync(Guid favoriteId)
        {
            await _favoriteTopicListService.DeleteAsync(favoriteId);
            return GetActionResponse(new IPMSResponse<object>());
        }
        [HttpGet("{listId}")]
        public async Task<IActionResult> GetInListAsync(Guid listId)
        {
            var response = await _favoriteTopicListService.GetInFavoriteAsync(listId);
            return GetActionResponse(new IPMSResponse<IList<GetFavoriteTopicResponse>>()
            {
                Data = response
            });
        }
    }
}
