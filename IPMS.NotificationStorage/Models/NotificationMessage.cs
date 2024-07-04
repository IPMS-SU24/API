using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace IPMS.NotificationStorage.Models
{
    public class NotificationMessage
    {
        [JsonIgnore]
        public ObjectId _id { get; set; }
        [JsonIgnore]
        public Guid AccountId { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
    }
}
