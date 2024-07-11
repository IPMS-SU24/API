using IPMS.API.Common.Extensions;
using IPMS.Business.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IPMS.NotificationStorage.Models;
using IPMS.API.Responses;
using IPMS.Business.Requests.Notification;

namespace IPMS.API.Controllers
{
    public class NotificationController : ApiControllerBase
    {
        private readonly INotificationStorageService _notificationStorageService;

        public NotificationController(INotificationStorageService notificationStorageService)
        {
            _notificationStorageService = notificationStorageService;
        }
        [Authorize]
        [HttpPost("[action]")]
        public async Task<IActionResult> TokenAsync([FromBody] string fcmToken)
        {
            var userId = HttpContext.User.Claims.GetUserId();
            await _notificationStorageService.SaveUserTokenAsync(new UserToken()
            {
                FCMToken = fcmToken,
                UserId = userId
            });
            return GetActionResponse(new IPMSResponse<object>());
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> AllNotificationAsync()
        {
            var userId = HttpContext.User.Claims.GetUserId();
            var response = new IPMSResponse<IList<NotificationMessage>>()
            {
                Data = await _notificationStorageService.GetAllNotificationOfUserAsync(userId)
            };
            return GetActionResponse(response);
        }
        [Authorize]
        [HttpPost("[action]")]
        public async Task<IActionResult> MarkAsRead([FromBody] MarkAsReadRequest request)
        {
            var userId = HttpContext.User.Claims.GetUserId();
            await _notificationStorageService.MarkAsRead(request);
            return GetActionResponse(new IPMSResponse<object>());
        }
    }
}
