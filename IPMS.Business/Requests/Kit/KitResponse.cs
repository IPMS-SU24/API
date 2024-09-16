using IPMS.DataAccess.Models;

namespace IPMS.Business.Requests.Kit
{
    public class KitResponse
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public virtual ICollection<KitDevice> Devices { get; set; } = new List<KitDevice>();
    }
}
