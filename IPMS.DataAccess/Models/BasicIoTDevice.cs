using IPMS.DataAccess.Common.Models;

namespace IPMS.DataAccess.Models
{
    public class BasicIoTDevice : BaseModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public virtual ICollection<KitDevice> Kits { get; set; } = new List<KitDevice>();
    }
}