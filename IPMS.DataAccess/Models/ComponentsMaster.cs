using IPMS.DataAccess.Common.Enums;
using IPMS.DataAccess.Common.Models;

namespace IPMS.DataAccess.Models
{
    public class ComponentsMaster : BaseModel
    {
        public int Quantity { get; set; }
        public BorrowedStatus Status { get; set; }
        public ComponentsMasterType MasterType { get; set; }
        public string Comment { get; set; }
        public Guid? ComponentId { get; set; }
        public Guid? MasterId { get; set; }
        public virtual IoTComponent? Component { get; set; }


    }
}
