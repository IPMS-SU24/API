using IPMS.DataAccess.Common.Enums;
using IPMS.DataAccess.Common.Models;

namespace IPMS.DataAccess.Models
{
    public class MemberHistory : BaseModel
    {
        public Guid ReporterId { get; set; }
        public Guid? MemberSwapId { get; set; }
        public Guid? ProjectFromId { get; set; }
        public Guid? ProjectToId { get; set; }
        public RequestStatus IsProjectFromApproved { get; set; } = RequestStatus.Waiting;
        public RequestStatus IsProjectToApproved { get; set; } = RequestStatus.Waiting;
        public RequestStatus IsMemberSwapApproved { get; set; } = RequestStatus.Waiting;
        public string ProjectFromComment { get; set; }
        public string ProjectToComment { get; set; }
    }
}
