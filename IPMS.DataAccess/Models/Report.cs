using IPMS.DataAccess.Common.Models;

namespace IPMS.DataAccess.Models
{
    public class Report : BaseModel
    {
        public Guid? ReporterId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public bool Status { get; set; }
        public string ResponseContent { get; set; }
        public virtual IPMSUser? Reporter { get; set; }
    }
}
