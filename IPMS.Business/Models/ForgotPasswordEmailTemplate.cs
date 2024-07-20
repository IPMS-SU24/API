namespace IPMS.Business.Models
{
    public class ForgotPasswordEmailTemplate
    {
        public static string GetBody(string resetUrl) => string.Format(@"
            <p>You have just requested to reset your password. Please click the following button to reset your password:
            </p>
            <h2 style=""background: #FF5B21;margin: 0 auto;width: max-content;padding: 0 10px;color: #fff;border-radius: 4px;""><a href=""{0}"" style=""color: white; text-decoration: none;"">Reset Password</a></h2>
            </div>", resetUrl);
        public static string Subject => "IPMS Reset Password";
    }
}
