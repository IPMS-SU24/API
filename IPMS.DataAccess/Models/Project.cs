using IPMS.DataAccess.Common.Models;

namespace IPMS.DataAccess.Models
{
    public class Project : BaseModel
    {
        public int GroupNum { get; set; } = 1;
        public bool IsPublished { get; set; } = false;
        public decimal? Grade { get; set; }
        public Guid OwnerId { get; set; }
        public virtual IPMSUser Owner { get; set; } //lecturer
        public virtual ClassTopic? Topic => AssessmentTopic?.FirstOrDefault(x=>x.AssessmentId == null);
        public virtual ICollection<ClassTopic> AssessmentTopic { get; set; } = new List<ClassTopic>();
        public virtual ICollection<Student> Students {get; set;} = new List<Student>();
        public virtual ICollection<ProjectSubmission> Submissions {get; set;} = new List<ProjectSubmission>();
        public virtual ICollection<Topic> SuggestedTopic {get; set;} = new List<Topic>();
        public virtual ICollection<KitProject> Kits { get; set; } = new List<KitProject>();
    }
}
