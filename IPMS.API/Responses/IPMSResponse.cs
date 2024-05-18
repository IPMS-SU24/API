using IPMS.API.Common.Enums;
using IPMS.API.Common.Extensions;
using System.Xml.Linq;

namespace IPMS.API.Responses
{
    public class IPMSResponse<TData>
    {
        public ResponseStatus Status { get; set; } = ResponseStatus.Success;
        private string? _message;
        public string Message
        {
            get => _message ?? Status.GetResponseMessage();
            set => _message = value;
        }
        public IDictionary<string, string[]>? Errors { get; set; }
        public TData? Data { get; set; }
    }
}
