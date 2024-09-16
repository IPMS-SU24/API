
namespace IPMS.Business.Requests.Kit
{
    public class UpdateKitRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<KitDeviceRequest> Devices { get; set; } = new();
    }
}
