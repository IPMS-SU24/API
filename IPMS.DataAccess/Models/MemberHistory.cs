using IPMS.DataAccess.Common.Enums;
using IPMS.DataAccess.Common.Models;

namespace IPMS.DataAccess.Models
{
    public class MemberHistory : BaseModel
    {
        public Guid ReporterId { get; set; }
        public Guid IPMSClassId { get; set; }
        public Guid? MemberSwapId { get; set; }
        public Guid? ProjectFromId { get; set; }
        public Guid? ProjectToId { get; set; }
        public RequestStatus ProjectFromStatus { get; set; } = RequestStatus.Waiting;
        public RequestStatus ProjectToStatus { get; set; } = RequestStatus.Waiting;
        public RequestStatus MemberSwapStatus{ get; set; } = RequestStatus.Waiting;
        public string ProjectFromComment { get; set; } = "";
        public string ProjectToComment { get; set; } = "";
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
