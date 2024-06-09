using System.Text.Json.Serialization;

namespace IPMS.Business.Requests.IoTComponent
{
    public class IoTModelRequest
    {
        [JsonPropertyName("id")]
        public Guid ComponentId { get; set; }
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
    }
}
