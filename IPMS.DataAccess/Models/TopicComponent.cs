using IPMS.DataAccess.CommonModels;

namespace IPMS.DataAccess.Models
{
    public class TopicComponent : BaseModel
    {
        public int Quantity { get; set; }
        public BorrowedStatus Status { get; set; }
        public string Comment { get; set; }
        public Guid TopicId { get; set; }
        public Guid ComponentId { get; set; }
        public virtual IoTComponent Component { get; set; }
        public virtual Topic Topic { get; set; }
    }
}
