using IPMS.Business.Common.Exceptions;
using IPMS.Business.Interfaces.Services;
using IPMS.NotificationStorage;
using IPMS.NotificationStorage.Models;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace IPMS.Business.Services
{
    public class NotificationStorageService : INotificationStorageService
    {
        private readonly IPMSNotificationStorageContext _dbContext;
        public NotificationStorageService(IPMSNotificationStorageContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IList<NotificationMessage>> GetAllNotificationOfUserAsync(Guid userId)
        {
            var response =   await (await _dbContext.NotificationMessages.FindAsync(x=>x.AccountId == userId)).ToListAsync();
            if(response == null || !response.Any())
            {
                throw new DataNotFoundException();
            }
            return response.OrderByDescending(x=>x.DateSent).ToList();
        }

        public async Task SaveUserTokenAsync(UserToken userToken)
        {
            var isExist = await (await _dbContext.UserTokens.FindAsync(x => x.UserId == userToken.UserId && x.FCMToken == userToken.FCMToken)).AnyAsync();
            if (isExist) return;
            await _dbContext.UserTokens.InsertOneAsync(userToken);
        }
    }
}
