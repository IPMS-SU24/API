
using IPMS.Business.Pagination;

namespace IPMS.Business.Requests.Admin
{
    public class GetLecturerListRequest : BasePaginationRequest
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
    }
}
