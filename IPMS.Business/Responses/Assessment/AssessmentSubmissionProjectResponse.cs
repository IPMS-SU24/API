

using IPMS.Business.Responses.SubmissionModule;

namespace IPMS.Business.Responses.Assessment
{
    public class AssessmentSubmissionProjectResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<SubmissionModuleResponse> SubmissionModules { get; set; } = new List<SubmissionModuleResponse>(); 
    }
}
