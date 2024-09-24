namespace IPMS.Business.Requests.IoTComponent
{
    public class ReturnIoTComponentsRequest
    {
        public Guid ProjectId { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<IoTReview> IotComponents { get; set; } = new();
    }
}
