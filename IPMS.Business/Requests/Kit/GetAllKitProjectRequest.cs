using IPMS.Business.Pagination;

namespace IPMS.Business.Requests.Kit
{
    public class GetAllKitProjectRequest : BasePaginationRequest
    {
        public Guid? SemesterId { get; set; }
        public Guid? ClassId { get; set; }
        public Guid? ProjectId { get; set; }
    }
}
