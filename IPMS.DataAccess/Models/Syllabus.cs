using IPMS.DataAccess.CommonModels;

namespace IPMS.DataAccess.Models
{
    public class Syllabus : BaseModel
    {
        public string Name { get; set; }    
        public string? ShortName { get; set; }
        public string Description { get; set; }
        public virtual ICollection<Semester> Semesters { get; set; } = new List<Semester>();
        public virtual ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();
    }
}
