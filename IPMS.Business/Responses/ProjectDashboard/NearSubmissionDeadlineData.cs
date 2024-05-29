namespace IPMS.Business.Responses.ProjectDashboard
{
    public class NearSubmissionDeadlineData
    {
        public List<NearDealineSubmission> Submissions { get; set; } = new List<NearDealineSubmission>();
    }
    public class NearDealineSubmission
    {
        public string AssessmentId { get; set; }
        public string Name { get; set; }
        public DateTime EndDate { get; set; }
        public string SubmissionModuleId { get; set; }
    }
}
