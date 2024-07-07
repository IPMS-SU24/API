using IPMS.DataAccess.Common.Models;

namespace IPMS.DataAccess.Models
{
    public class SubmissionModule : BaseModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Percentage { get; set; }
        public Guid? SemesterId { get; set; }
        public Guid? AssessmentId { get; set; }
        public Guid? LectureId { get; set; }
        public virtual Assessment? Assessment { get; set; }
        public virtual Semester? Semester { get; set; }
        public virtual IPMSUser? Lecturer { get; set; }
        public virtual ICollection<ProjectSubmission> ProjectSubmissions { get; set; } = new List<ProjectSubmission>();
        public virtual ICollection<ClassModuleDeadline> ClassModuleDeadlines { get; set; } = new List<ClassModuleDeadline>();
       
    }
}
