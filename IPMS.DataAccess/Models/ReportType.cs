using IPMS.DataAccess.Common.Models;

namespace IPMS.DataAccess.Models
{
    public class ReportType : BaseModel
    {
        public string Name { get; set; }
        public virtual ICollection<Report> Reports { get; set;} = new List<Report>();
    }
}
