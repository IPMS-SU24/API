namespace IPMS.Business.Requests.Kit
{
    public class KitResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public virtual ICollection<DeviceInformation> Devices { get; set; } = new List<DeviceInformation>();
    }

    public class DeviceInformation
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
    }

}
