namespace IPMS.Business.Requests.ProjectSubmission
{
    public class GradeSubmissionRequest
    {
        public Guid SubmissionId { get; set; }
        public decimal Grade { get; set; }
        public string? Response { get; set; }
    }
}
