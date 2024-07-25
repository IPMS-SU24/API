using IPMS.Business.Pagination;
namespace IPMS.Business.Requests.Assessment
{
    public class GetAllAssessmentsRequest : BasePaginationRequest
    {
        public Guid Id { get; set; }
        public string? Name {get; set;}
    }
}
