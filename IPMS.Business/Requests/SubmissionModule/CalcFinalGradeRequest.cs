
namespace IPMS.Business.Requests.SubmissionModule
{
    public class CalcFinalGradeRequest
    {
        public Guid ClassId { get; set; }
        public Guid GroupId { get; set; }
        public List<StudentFinalPer> Members { get; set; } = new List<StudentFinalPer>();
    }
    public class StudentFinalPer
    {
        public Guid StudentId { get; set; }
        public decimal FinalPercentage { get; set; }
    }
}

