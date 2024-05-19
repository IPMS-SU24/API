using IPMS.DataAccess.CommonModels;

namespace IPMS.DataAccess.Models
{
    public class IoTComponent : BaseModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageLink { get; set; }
        public virtual ICollection<ProjectComponent> Projects { get; set; } = new List<ProjectComponent>();
        public virtual ICollection<TopicComponent> Topics { get; set; } = new List<TopicComponent>();
        public virtual ICollection<IPMSUser> Lecturers { get; set; } = new List<IPMSUser>();
    }
}
