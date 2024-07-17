using IPMS.DataAccess.Common.Models;

namespace IPMS.DataAccess.Models
{
    public class LecturerGrade : BaseModel
    {
        public Guid CommitteeId { get; set; }
        public Guid SubmissionId { get; set; }
        public decimal? Grade { get; set; }
        public virtual Committee Committee { get; set; }
        public virtual ProjectSubmission Submission { get; set; }
    }
}
