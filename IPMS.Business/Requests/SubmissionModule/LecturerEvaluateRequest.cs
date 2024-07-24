

namespace IPMS.Business.Requests.SubmissionModule
{
    public class LecturerEvaluateRequest
    {
        public Guid ClassId { get; set; }
        public Guid GroupId { get; set; }
        public List<StudentInfor> Members { get; set; } = new List<StudentInfor>();
    }
    public class StudentInfor
    {
        public Guid StudentId { get; set; }
        public decimal Percentage { get; set; }
    }

    public class AssessmentSubmission
    {
        public Guid Id { get; set; }
        public decimal Percentage { get; set; }
        public ICollection<DataAccess.Models.SubmissionModule> Submissions { get; set; } = new List<DataAccess.Models.SubmissionModule>();
    }

}
