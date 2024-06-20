using IPMS.Business.Pagination;

namespace IPMS.Business.Requests.ProjectPreference
{
    public class ProjectPreferenceRequest : BasePaginationRequest
    {
        public string? SearchValue { get; set; }
        public Guid? LecturerId { get; set; }
        public string? SemesterCode { get; set; }
    }
}
