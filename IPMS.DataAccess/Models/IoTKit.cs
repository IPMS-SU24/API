using IPMS.DataAccess.Common.Models;

namespace IPMS.DataAccess.Models
{
    public class IoTKit : BaseModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public virtual ICollection<KitDevice> Devices { get; set; } = new List<KitDevice>();
        public virtual ICollection<KitProject> Projects { get; set; } = new List<KitProject>();
    }
}
