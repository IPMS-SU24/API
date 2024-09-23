namespace IPMS.Business.Requests.ProjectKit
{
    public class CreateKitProjectRequest
    {
        public DateTime BorrowedDate { get; set; }
        public string? Comment { get; set; }
        public Guid ProjectId { get; set; }
        public Guid KitId { get; set; }
        public Guid StudentId { get; set; }
    }
}
