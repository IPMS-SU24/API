
namespace IPMS.Business.Requests.SubmissionModule
{
    public class ConfigureSubmissionModuleRequest
    {
        public List<SubmissionModuleRequest> SubmissionModules { get; set; } = new();
        public Guid AssessmentId { get; set; }

    }

    public class SubmissionModuleRequest
    {
        public Guid ModuleId { get; set; } = Guid.Empty;
        public string ModuleName { get; set; }
        public string? Description { get; set; }
        public decimal Percentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
