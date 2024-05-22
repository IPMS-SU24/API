namespace IPMS.Business.Models
{
    public class NotificationMessage
    {
        public Guid AccountId { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
    }
}
