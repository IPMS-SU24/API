using AutoFilterer.Attributes;
using AutoFilterer.Enums;
using IPMS.Business.Pagination;

namespace IPMS.Business.Requests.ClassTopic
{
    public class GetClassTopicRequest : BasePaginationAutoFiltererRequest
    {
        public string? SearchValue { get; set; } 
    }
}
