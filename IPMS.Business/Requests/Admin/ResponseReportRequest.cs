
using IPMS.DataAccess.Common.Enums;

namespace IPMS.Business.Requests.Admin
{
    public class ResponseReportRequest
    {
        public Guid? Id { get; set; }
        public string ResponseContent { get; set; }
        public RequestStatus Status { get; set; }
    }
}
