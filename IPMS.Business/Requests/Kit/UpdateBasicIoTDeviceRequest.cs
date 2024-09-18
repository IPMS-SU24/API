namespace IPMS.Business.Requests.Kit
{
    public class UpdateBasicIoTDeviceRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
