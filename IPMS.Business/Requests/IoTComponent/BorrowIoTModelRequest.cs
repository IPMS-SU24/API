using System.Text.Json.Serialization;

namespace IPMS.Business.Requests.IoTComponent
{
    public class BorrowIoTModelRequest
    {
        [JsonPropertyName("Id")]
        public Guid ComponentId { get; set; }
        [JsonPropertyName("Quantity")]
        public int Quantity { get; set; }
    }
}
