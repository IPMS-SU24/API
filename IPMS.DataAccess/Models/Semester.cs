using IPMS.DataAccess.Common.Models;

namespace IPMS.DataAccess.Models
{
    public class Semester : BaseModel
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid? SyllabusId { get; set; }
        public virtual Syllabus? Syllabus { get; set; }
        public virtual ICollection<SubmissionModule> Modules { get; set; } = new List<SubmissionModule>();
        public virtual ICollection<IPMSClass> Classes { get; set; } = new List<IPMSClass>();
    }
}
