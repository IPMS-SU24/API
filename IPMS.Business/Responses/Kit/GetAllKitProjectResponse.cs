namespace IPMS.Business.Responses.Kit
{
    public class GetAllKitProjectResponse
    {
        public Guid Id { get; set; }
        public DateTime BorrowedDate { get; set; }
        public DateTime? ReturnedDate { get; set; }
        public string? Comment { get; set; }
        public Guid ProjectId { get; set; }
        public Guid KitId { get; set; }
        public string KitName { get; set; }
        public int ProjectNum { get; set; }
        public Guid ClassId { get; set; }
        public string ClassName { get; set; } 
        public Guid BorrowerId { get; set; }
        public string BorrowerName { get; set; } 
        public string BorrowerEmail { get; set; }
        public string? ReturnerName { get; set; }
        public string? ReturnerEmail { get; set; }
    }
}
