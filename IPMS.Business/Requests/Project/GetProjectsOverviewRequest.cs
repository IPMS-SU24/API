using IPMS.Business.Pagination;
namespace IPMS.Business.Requests.Project
{
    public class GetProjectsOverviewRequest : BasePaginationRequest
    {
        public Guid ClassId { get; set; }
    }
}
