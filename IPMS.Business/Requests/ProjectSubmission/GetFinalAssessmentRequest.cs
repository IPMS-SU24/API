
using IPMS.Business.Pagination;

namespace IPMS.Business.Requests.ProjectSubmission
{
    public class GetFinalAssessmentRequest : BasePaginationRequest
    {
        public Guid ClassId { get; set; }
        public Guid GroupId { get; set; }
    }
}
