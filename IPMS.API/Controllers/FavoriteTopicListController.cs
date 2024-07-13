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
    public class FavoriteTopicListController : ApiControllerBase
    {
        private readonly IFavoriteTopicListService _favoriteTopicListService;

        public FavoriteTopicListController(IFavoriteTopicListService favoriteTopicListService)
        {
            _favoriteTopicListService = favoriteTopicListService;
        }
        [EnumAuthorize(UserRole.Lecturer)]
        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateFavoriteTopicListRequest request)
        {
            var response = new IPMSResponse<CreateFavoriteTopicListResponse>
            {
                Data = await _favoriteTopicListService.Create(request, HttpContext.User.Claims.GetUserId())
            };
            return GetActionResponse(response);
        }
    }
}
