using IPMS.DataAccess.CommonModels;

namespace IPMS.DataAccess.Models
{
    public class LecturerGrade : BaseModel
    {
        public Guid CommitteeId { get; set; }
        public decimal Grade { get; set; }
        public virtual Committee Committee { get; set; }
    }
}
