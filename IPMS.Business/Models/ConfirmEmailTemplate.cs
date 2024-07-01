namespace IPMS.Business.Common.Models
{
    public static class ConfirmEmailTemplate
    {
        public static string GetBody(string confirmUrl, string password)  => string.Format(@"<div style=""font-family: Helvetica,Arial,sans-serif;min-width:1000px;overflow:auto;line-height:2"">
          <div style=""margin:50px auto;width:70%;padding:20px 0"">
            <div style=""border-bottom:1px solid #eee"">
              <a href="""" style=""font-size:1.4em;color: #FF5B21;text-decoration:none;font-weight:600"">IPMS</a>
            </div>
            <p style=""font-size:1.1em"">Hi,</p>
            <p>You are added in new IOT102 Class. We have just create an IPMS account to use our system for manage your project with default password:{0}.
                Please confirm your email by click the following button:
            </p>
            <h2 style=""background: #FF5B21;margin: 0 auto;width: max-content;padding: 0 10px;color: #fff;border-radius: 4px;""><a href=""{1}"">Confirm Account</a></h2>
            <p style=""font-size:0.9em;"">Best regards,<br />IPMS</p>
            <hr style=""border:none;border-top:1px solid #eee"" />
            <div style=""float:right;padding:8px 0;color:#aaa;font-size:0.8em;line-height:1;font-weight:300"">
              <p>IPMS Inc</p>
              <p>HCM</p>
              <p>0123456789</p>
            </div>
          </div>
        </div>", password, confirmUrl);
        public static string Subject => "IPMS Account Comfirmation";
    }
}
