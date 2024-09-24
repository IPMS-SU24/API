
namespace IPMS.Business.Requests.Class
{
    public class UpdateClassDeadlineRequest
    {
        public Guid ClassId { get; set; }
        public DateTime CreateGroup {  get; set; }
        public DateTime ChangeGroup { get; set; }
        public DateTime? ChangeTopic { get; set; }
        public DateTime? BorrowIot { get; set; }
    }
}
