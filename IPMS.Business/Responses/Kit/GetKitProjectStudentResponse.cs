namespace IPMS.Business.Responses.Kit
{
    public class GetKitProjectStudentResponse
    {
        public DateTime BorrowedDate { get; set; }
        public DateTime? ReturnedDate { get; set; }
        public string? Comment { get; set; }
        public string KitName { get; set; }
        public string KitDescription { get; set; }
        public List<KitDeviceResponse> Devices { get; set; } = new();
    }
    public class KitDeviceResponse
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
    }
}
