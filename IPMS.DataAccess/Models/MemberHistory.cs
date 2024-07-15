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
        public Guid ProjectToId { get; set; }
        public RequestStatus? ProjectFromStatus { get; set; }
        public RequestStatus ProjectToStatus { get; set; } = RequestStatus.Waiting;
        public RequestStatus? MemberSwapStatus { get; set; }
        public RequestStatus FinalStatus
        {
            get
            {
                //Join Request
                if(!ProjectFromId.HasValue && !ProjectFromStatus.HasValue && !MemberSwapId.HasValue && !MemberSwapStatus.HasValue)
                {
                    return ProjectToStatus;
                }
                //else Swap Request
                if(ProjectToStatus == RequestStatus.Rejected || ProjectFromStatus!.Value == RequestStatus.Rejected || MemberSwapStatus!.Value == RequestStatus.Rejected)
                {
                    return RequestStatus.Rejected;
                }
                if(ProjectToStatus == RequestStatus.Approved && ProjectFromStatus!.Value == RequestStatus.Approved && MemberSwapStatus!.Value == RequestStatus.Approved)
                {
                    return RequestStatus.Approved;
                }
                return RequestStatus.Waiting;
            }
        }
        public string ProjectFromComment { get; set; } = string.Empty;
        public string ProjectToComment { get; set; } = string.Empty;
    }
}
