
namespace IPMS.Business.Responses.Admin
{
    public class GetAssessmentDetailResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
        public decimal Percentage { get; set; }
        public Guid SyllabusId { get; set; }
        public string SyllabusName { get; set; }
    }
}
