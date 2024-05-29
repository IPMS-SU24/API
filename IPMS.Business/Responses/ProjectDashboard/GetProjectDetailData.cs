namespace IPMS.Business.Responses.ProjectDashboard
{
    public class GetProjectDetailData
    {
        public Guid ProjectId { get; set; }
        public string TopicName { get; set; } = null!;
        public List<AssessmentDetail> Assessements { get; set; } = new List<AssessmentDetail>();
        public SubmissionCount Submission { get; set; } = new();
    }
    public class SubmissionCount
    {
        public int Done { get; set; }
        public int Total { get; set; }
    }
    public class AssessmentDetail
    {
        public Guid AssessmentId { get; set; }
        public string AssessmentName { get; set; } = null!;
        public string Status { get; set; } = null!;
    }
}
