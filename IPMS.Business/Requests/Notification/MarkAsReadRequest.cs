using MongoDB.Bson;

namespace IPMS.Business.Requests.Notification
{
    public class MarkAsReadRequest
    {
        public IList<string> NotificationIds { get; set; } = new List<string>();
    }
}
