using IPMS.Business.Pagination;

namespace IPMS.Business.Requests.ClassTopic
{
    public class GetClassTopicRequest : BasePaginationAutoFiltererRequest
    {
        public string? SearchValue { get; set; } 
        public Guid? AssessmentId { get; set; } 
    }
}
