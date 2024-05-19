using IPMS.DataAccess.CommonModels;

namespace IPMS.DataAccess.Models
{
    public class MemberHistory : BaseModel
    {
        public Guid ReporterId { get; set; }
        public Guid MemberSwapId { get; set; }
        public Guid ProjectFromId { get; set; }
        public Guid ProjectToId { get; set; }
        public bool IsProjectFromApproved { get; set; }
        public bool IsProjectToApproved { get; set; }
        public string ProjectFromComment { get; set; }
        public string ProjectToComment { get; set; }
    }
}
