
namespace IPMS.Business.Responses.Assessment
{
    public class ConfigureAssessmentsRequest
    {
        public List<AssessmentInfo> Assessments { get; set; } = new();
        public Guid SyllabusId { get; set; }
    }

    public class AssessmentInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Percentage { get; set; }
        public int Order { get; set; }
        public bool IsDelete { get; set; }
    }
}
