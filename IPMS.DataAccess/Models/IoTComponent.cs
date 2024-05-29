using IPMS.DataAccess.Common.Models;

namespace IPMS.DataAccess.Models
{
    public class IoTComponent : BaseModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageLink { get; set; }
        public virtual ICollection<ComponentsMaster> ComponentsMasters { get; set; } = new List<ComponentsMaster>();
    }
}