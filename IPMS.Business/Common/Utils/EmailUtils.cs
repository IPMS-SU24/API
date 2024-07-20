namespace IPMS.Business.Common.Utils
{
    public static class EmailUtils
    {
        private const string footer = @"<p style=""font-size:0.9em;"">Best regards,<br />IPMS</p>
            <hr style=""border:none;border-top:1px solid #eee"" />
            <div style=""float:right;padding:8px 0;color:#aaa;font-size:0.8em;line-height:1;font-weight:300"">
              <p>IPMS Inc</p>
              <p>HCM</p>
              <p>0123456789</p>
            </div>";
        public static string GetFullMailContent(string body) => string.Format(@"<div style=""font-family: Helvetica,Arial,sans-serif;min-width:1000px;overflow:auto;line-height:2"">
          <div style=""margin:50px auto;width:70%;padding:20px 0"">
            <div style=""border-bottom:1px solid #eee"">
              <a href="""" style=""font-size:1.4em;color: #FF5B21;text-decoration:none;font-weight:600"">IPMS</a>
            </div>
            <div>
            <p style=""font-size:1.1em"">Hi,</p>
            {0}
            </div>            
           {1}
          </div>
        </div>", body, footer);
    }
}
