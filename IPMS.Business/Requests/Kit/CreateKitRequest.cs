
namespace IPMS.Business.Requests.Kit
{
    public class CreateKitRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<KitDeviceRequest> Devices { get; set; } = new();
    }
    public class KitDeviceRequest
    {
        public Guid DeviceId { get; set; }
        public string Quantity { get; set; }
    }
}
