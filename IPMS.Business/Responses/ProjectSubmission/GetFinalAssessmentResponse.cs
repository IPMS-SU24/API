
namespace IPMS.Business.Responses.ProjectSubmission
{
    public class GetFinalAssessmentResponse
    {
        public Guid ModuleId { get; set; }
        public Guid SubmissionId { get; set; }
        public string ModuleName { get; set; }
        public decimal Percentage { get; set; }
        public string Description { get; set; }
        public string FileLink { get; set; }
        public decimal Grade { get; set; }
        public string? Response { get; set; }
    }
}
