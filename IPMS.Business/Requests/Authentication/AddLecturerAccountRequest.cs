namespace IPMS.Business.Requests.Authentication
{
    public class AddLecturerAccountRequest
    {
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Phone { get; set; } = null!;
    }
}
