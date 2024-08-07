using IPMS.Business.Pagination;
namespace IPMS.Business.Requests.Project
{
    public class GetProjectDetailRequest
    {
        public Guid ClassId { get; set; }
        public Guid GroupId { get; set; }
        public bool? IsCommittee { get; set; }
    }
}
