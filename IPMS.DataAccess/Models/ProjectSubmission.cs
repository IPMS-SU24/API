using IPMS.DataAccess.Common.Models;

namespace IPMS.DataAccess.Models
{
    public class ProjectSubmission : BaseModel
    {
        public string Name { get; set; }
        public DateTime SubmissionDate { get; set; }
        public decimal? FinalGrade { get; set; }
        public Guid? ProjectId { get; set; }
        public Guid? SubmissionModuleId { get; set; }
        public Guid? SubmitterId { get; set; }
        public virtual IPMSUser? Submitter { get; set; }
        public virtual Project? Project { get; set; }
        public virtual SubmissionModule? SubmissionModule { get; set; }
        public virtual ICollection<LecturerGrade> Grades {get; set;} = new List<LecturerGrade>();
        
    }
}
