using Newtonsoft.Json;

namespace IPMS.Business.Models
{
    public class IPMSMailMessage
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<string>? MailTo { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<string>? MailCc { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<string>? MailBcc { get; set; }
        public string Subject { get; set; } = null!;
        public string Body { get; set; } = null!;
    }
}
