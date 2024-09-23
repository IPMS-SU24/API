
namespace IPMS.Business.Requests.KitProject
{
    public class UpdateKitProjectRequest
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public string? Comment { get; set; }
    }
}
