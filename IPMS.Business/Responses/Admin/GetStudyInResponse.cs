
namespace IPMS.Business.Responses.Admin
{
    public class GetStudyInResponse
    {
        public string Role { get; set; }
        public Guid? ClassId { get; set; }
        public string Class { get; set; }
        public Guid? ProjectId { get; set; }
        public string Project { get; set; }
    }
}
