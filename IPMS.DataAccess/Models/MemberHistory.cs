using IPMS.DataAccess.Common.Models;

namespace IPMS.DataAccess.Models
{
    public class MemberHistory : BaseModel
    {
        public Guid ReporterId { get; set; }
        public Guid? MemberSwapId { get; set; }
        public Guid ProjectFromId { get; set; }
        public Guid? ProjectToId { get; set; }
        public bool IsProjectFromApproved { get; set; } = false;
        public bool IsProjectToApproved { get; set; } = false;
        public bool IsMemberSwapApproved { get; set; } = false;
        public string ProjectFromComment { get; set; }
        public string ProjectToComment { get; set; }
    }
}
