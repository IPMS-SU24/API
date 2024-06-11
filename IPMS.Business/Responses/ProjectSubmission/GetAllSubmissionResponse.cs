
namespace IPMS.Business.Responses.ProjectSubmission
{
    public class GetAllSubmissionResponse
    {
        public string ModuleName { get; set; } 
        public string AssesmentName { get; set; }
        public DateTime SubmitDate { get; set; }
        public string SubmitterName { get; set; }
        public Guid? SubmitterId { get; set; }
        public decimal? Grade { get; set; }
        public string Link { get; set; }
        public string FileName { get; set; }
        public bool IsNewest { get; set; }
        public Guid? AssessmentId { get; set; }
        public Guid? ModuleId { get; set; }

    }
}
