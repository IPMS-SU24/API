using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace IPMS.NotificationStorage.Models
{
    public class NotificationMessage
    {
        [JsonIgnore]
        public ObjectId _id { get; set; }
        [BsonIgnore]
        public string Id => _id.ToString();
        [JsonIgnore]
        public Guid AccountId { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public DateTime DateSent { get; init; }
        public bool MarkAsRead { get; set; }
    }
}
