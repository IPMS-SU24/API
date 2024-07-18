using IPMS.Business.Pagination;

namespace IPMS.Business.Requests.SubmissionModule
{
    public class GetSubmissionsRequest : BasePaginationRequest
    {
        public Guid ClassId { get; set; }
        public Guid ModuleId { get; set; }

    }
}
