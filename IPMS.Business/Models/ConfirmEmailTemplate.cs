namespace IPMS.Business.Models
{
    public static class ConfirmEmailTemplate
    {
        public static string GetBody(string confirmUrl, string password)  => string.Format(@"
            <p>You are added in new IoT102 Class. We have just create an IPMS account to use our system for manage your project with default password:<b>{0}</b>.
                Please confirm your email by click the following button:
            </p>
            <h2 style=""background: #FF5B21;margin: 0 auto;width: max-content;padding: 0 10px;color: #fff;border-radius: 4px;""><a href=""{1}"" style=""color: white; text-decoration: none;"">Confirm Account</a></h2>
            ", password, confirmUrl);
        public static string Subject => "IPMS Account Comfirmation";
    }
}
