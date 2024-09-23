using IPMS.DataAccess.Common.Models;

namespace IPMS.DataAccess.Models
{
    public class KitProject : BaseModel
    {
        public DateTime BorrowedDate { get; set; }
        public DateTime? ReturnedDate { get; set; }
        public string? Comment { get; set; }
        public Guid ProjectId { get; set; }
        public Guid KitId { get; set; }
        public virtual Project Project { get; set; }
        public virtual IoTKit Kit { get; set; }
        public Guid BorrowerId { get; set; }
        public virtual Student Borrower { get; set; }
        public Guid? ReturnerId { get; set; }
        public virtual Student? Returner { get; set; }
    }
}
