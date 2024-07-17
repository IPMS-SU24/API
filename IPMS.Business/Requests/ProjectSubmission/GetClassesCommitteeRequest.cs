
using IPMS.Business.Pagination;

namespace IPMS.Business.Requests.ProjectSubmission
{
    public class GetClassesCommitteeRequest : BasePaginationRequest
    {
        public string SemesterCode { get; set; }
    }
}
