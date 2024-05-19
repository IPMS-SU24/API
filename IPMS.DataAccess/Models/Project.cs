using IPMS.DataAccess.CommonModels;

namespace IPMS.DataAccess.Models
{
    public class Project : BaseModel
    {
        public string GroupName { get; set; }
        public bool IsPublished { get; set; }
        public decimal Grade { get; set; }
        public Guid OwnerLecturerId { get; set; }
        public Guid? ClassTopicId { get; set; }
        public virtual IPMSUser OwnerLecturer { get; set; }
        public virtual ClassTopic? ClassTopic { get; set; }
        public virtual ICollection<ProjectComponent> ProjectComponents { get; set; } = new List<ProjectComponent>();
        public virtual ICollection<MemberProject> MemberProjects {get; set;} = new List<MemberProject>();
        public virtual ICollection<ProjectSubmission> ProjectSubmissions {get; set;} = new List<ProjectSubmission>();
        
    }
}
