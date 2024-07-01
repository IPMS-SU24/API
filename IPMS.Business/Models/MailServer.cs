using Azure.Communication.Email;

namespace IPMS.Business.Common.Models
{
    public class MailServer
    {
        public EmailClient Client { get; }
        public MailServer(string connectionString)
        {
            Client = new EmailClient(connectionString);
        }
    }
}
