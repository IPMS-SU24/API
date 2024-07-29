
namespace IPMS.Business.Requests.IoTComponent
{
    public class UpdateIoTDeviceRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}
