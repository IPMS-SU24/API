namespace IPMS.Business.Requests.Admin
{
    public class CreateSemesterRequest
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid SyllabusId { get; set; }
    }
}
