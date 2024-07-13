using IPMS.Business.Pagination;

namespace IPMS.Business.Requests.IoTComponent
{
    public class GetBorrowIoTComponentsRequest : BasePaginationRequest
    {
        public bool OrderBy { get; set; } = true;
        public List<Guid> ClassIds { get; set; } = new();
    }   
}
