
namespace IPMS.Business.Requests.Admin
{
    public class UpdateSyllabusRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? ShortName { get; set; }
        public string Description { get; set; }

    }
}
