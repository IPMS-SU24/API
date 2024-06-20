using IPMS.DataAccess.Common.Enums;
using IPMS.DataAccess.Common.Models;

namespace IPMS.DataAccess.Models
{
    public class Report : BaseModel
    {
        public Guid? ReporterId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string? ResponseContent { get; set; }
        public RequestStatus Status { get; set; } = RequestStatus.Waiting;
        public string? FileName { get; set; }
        public Guid? ReportTypeId { get; set; }
        public virtual IPMSUser? Reporter { get; set; }
        public virtual ReportType? ReportType { get; set; }
    }
}
