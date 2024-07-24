using IPMS.DataAccess.Common.Models;

namespace IPMS.DataAccess.Models
{
    public class Student : BaseModel
    {
        public decimal? ContributePercentage { get; set; }
        public decimal? FinalPercentage { get; set; }
        public decimal? FinalGrade { get; set; }
        public Guid? ProjectId { get; set; }
        public Guid InformationId { get; set; }
        public Guid ClassId { get; set; }
        public virtual IPMSUser Information { get; set; } = null!;
        public virtual IPMSClass Class { get; set; } = null!;
        public virtual Project? Project { get; set; }
        public int? JobImportId { get; set; }
    }
}
