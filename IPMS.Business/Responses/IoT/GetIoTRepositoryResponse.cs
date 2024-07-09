namespace IPMS.Business.Responses.IoT
{

    public class GetIoTRepositoryResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int TotalQuantity { get; set; }
        public IEnumerable<BorrowInGroup> Components { get; set; } = new List<BorrowInGroup>();
    }

    public class BorrowInGroup
    {
        public string ClassCode { get; set; }
        public int GroupNumber { get; set; }
        public int BorrowNumber { get; set; }
    }
}
