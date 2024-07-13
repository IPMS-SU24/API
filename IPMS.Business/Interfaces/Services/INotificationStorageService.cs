using IPMS.Business.Requests.Notification;
using IPMS.NotificationStorage.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPMS.Business.Interfaces.Services
{
    public interface INotificationStorageService
    {
        Task SaveUserTokenAsync(UserToken userToken);
        Task<IList<NotificationMessage>> GetAllNotificationOfUserAsync(Guid userId);
        Task MarkAsRead(MarkAsReadRequest request);
    }
}
