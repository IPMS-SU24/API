using IPMS.Business.Pagination;

namespace IPMS.Business.Requests.Class

{
    public class GetClassListRequest : BasePaginationRequest
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public Guid SemesterId { get; set; }
    }
}
