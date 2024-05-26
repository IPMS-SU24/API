using IPMS.DataAccess.Common.Models;

namespace IPMS.DataAccess.Models
{
    public class Project : BaseModel
    {
        public string GroupName { get; set; }
        public bool IsPublished { get; set; } = false;
        public decimal? Grade { get; set; }
        public Guid? OwnerId { get; set; }
        public virtual IPMSUser? Owner { get; set; }
        public virtual ClassTopic? Topic { get; set; }
        public virtual ICollection<Student> Students {get; set;} = new List<Student>();
        public virtual ICollection<ProjectSubmission> Submissions {get; set;} = new List<ProjectSubmission>();
        
    }
}
