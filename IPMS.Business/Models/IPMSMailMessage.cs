using System.Text.Json.Serialization;

namespace IPMS.Business.Models
{
    public class IPMSMailMessage
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IEnumerable<string>? MailTo { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IEnumerable<string>? MailCc { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IEnumerable<string>? MailBcc { get; set; }
        public string Subject { get; set; } = null!;
        public string Body { get; set; } = null!;
    }
}
