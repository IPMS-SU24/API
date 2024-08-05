
namespace IPMS.Business.Responses.Admin
{
    public class GetSemesterDetailResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid SyllabusId { get; set; }
        public string SyllabusName { get; set; }

    }
}
