
using IPMS.Business.Responses.ProjectSubmission;

namespace IPMS.Business.Responses.SubmissionModule
{
    public class SubmissionModuleResponse
    {
        public Guid ModuleId { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; }
        public virtual ICollection<ProjectSubmissionResponse> ProjectSubmissions { get; set; } = new List<ProjectSubmissionResponse>();

    }
}
