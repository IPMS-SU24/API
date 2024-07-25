
using IPMS.Business.Pagination;

namespace IPMS.Business.Requests.IoTComponent
{
    public class GetAllIotComponentsRequest : BasePaginationRequest
    {
        public string? SearchValue { get; set; }
    }
}
