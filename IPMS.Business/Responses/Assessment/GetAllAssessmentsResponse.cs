namespace IPMS.Business.Responses.Assessment
{
    public class GetAllAssessmentsResponse
    {
        public Guid Id { get; set; }
        public int Order { get; set; }
        public string Name { get; set; }
        public decimal Percentage { get; set; }
        public string Description { get; set; }
        public string SyllabusName { get; set; }
        public bool IsEdited { get; set; }
    }
}
