using IPMS.DataAccess.Common.Models;

namespace IPMS.DataAccess.Models
{
    public class KitDevice : BaseModel
    {
        public int Quantity { get; set; }
        public Guid KitId { get; set; }
        public Guid DeviceId { get; set; }
        public virtual IoTKit Kit { get; set; }
        public virtual BasicIoTDevice Device { get; set; }
    }
}
