
namespace IPMS.Business.Requests.Authentication
{
    public class UpdateLecturerAccountRequest
    {
        public Guid? Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Phone { get; set; } = null!;
    }
}
