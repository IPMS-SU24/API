using IPMS.DataAccess.CommonModels;

namespace IPMS.DataAccess.Models
{
    public class AccountComponent : BaseModel
    {
        public int Quantity { get; set; }
        public string Comment { get; set; }
        public BorrowedStatus Status { get; set; }
        public Guid LecturerId { get; set; }
        public Guid ComponentId { get; set; }
        public virtual IoTComponent Component { get; set; }
        public virtual IPMSUser Lecturer { get; set; }

    }
}
