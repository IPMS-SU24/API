using IPMS.DataAccess.CommonModels;

namespace IPMS.DataAccess.Models
{
    public class ProjectComponent : BaseModel
    {
        public int Quantity { get; set; }
        public BorrowedStatus Status { get; set; }
        public string Comment { get; set; }
        public Guid ComponentId { get; set; }
        public Guid ProjectId { get; set; }
        public virtual IoTComponent Component { get; set; }
        public virtual Project Project { get; set; }


    }
}
