namespace IPMS.Business.Requests.Authentication
{
    public class ResetPasswordRequest
    {
        public Guid UserId { get; set; }
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }
}
