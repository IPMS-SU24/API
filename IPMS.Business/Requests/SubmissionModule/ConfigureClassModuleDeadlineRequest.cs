
namespace IPMS.Business.Requests.SubmissionModule

{
    public class ConfigureClassModuleDeadlineRequest
    {
        public Guid ClassId { get; set; }
        public Guid SubmissionModuleId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
