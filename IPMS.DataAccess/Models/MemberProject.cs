using IPMS.DataAccess.CommonModels;

namespace IPMS.DataAccess.Models
{
    public class MemberProject : BaseModel
    {
        public Guid ProjectId { get; set; }
        public virtual Project Project { get; set; }
        public Guid StudentId { get; set; }
        public virtual IPMSUser Student { get; set; }
        public decimal ContributePercentage { get; set; }
    }
}
