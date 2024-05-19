using IPMS.DataAccess.CommonModels;

namespace IPMS.DataAccess.Models
{
    public class Assessment : BaseModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
        public decimal Percentage { get; set; }
        public Guid SyllabusId { get; set; }
        public virtual Syllabus Syllabus { get; set; }
        public virtual ICollection<SubmissionModule> Submissions { get; set; } = new List<SubmissionModule>();

    }
}
