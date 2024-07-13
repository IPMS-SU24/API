
namespace IPMS.Business.Requests.IoTComponent
{
    public class ReviewBorrowIoTComponentsRequest
    {
        public Guid ProjectId { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<IoTReview> IotComponents { get; set; } = new();
    }
    public class GroupIotReview
    {
        public DateTime CreatedAt { get; set; }
        public List<IoTReview> IotComponents { get; set; } = new();

    }
    public class IoTReview
    {
        public Guid Id { get; set; }
        public int Quantity { get; set; }
    }
}
