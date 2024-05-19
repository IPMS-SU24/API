using IPMS.DataAccess.CommonModels;

namespace IPMS.DataAccess.Models
{
    public class TopicComponent
    {
        public int Quantity { get; set; }
        public BorrowedStatus Status { get; set; }
        public string Comment { get; set; }
        public Guid TopicId { get; set; }
        public Guid ComponentId { get; set; }
        public virtual ICollection<IoTComponent> Components { get; set; } = new List<IoTComponent>();
        public virtual ICollection<Topic> Topics { get; set; } = new List<Topic>();
    }
}
