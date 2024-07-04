using IPMS.NotificationStorage.Models;
using MongoDB.Driver;

namespace IPMS.NotificationStorage
{
    public class IPMSNotificationStorageContext
    {
        private readonly IMongoDatabase _database;
        public IPMSNotificationStorageContext(string connectionString)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase("Notification");
        }
        public IMongoCollection<UserToken> UserTokens => _database.GetCollection<UserToken>("UserToken");
        public IMongoCollection<NotificationMessage> NotificationMessages => _database.GetCollection<NotificationMessage>("NotificationMessage");

    }
}
