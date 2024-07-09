
using IPMS.DataAccess.Common.Enums;

namespace IPMS.Business.Responses.Topic
{
    public class SuggestedTopicsResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public int? GroupNum { get; set; }
        public string Description { get; set; }
        public string Detail { get; set; }
        public DateTime CreateAt { get; set; }
        public RequestStatus Status { get; set; }
        public List<TopicIoT> Iots { get; set; } = new();
    }

    public class TopicIoT 
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
    }

    public class GroupInformation
    {
        public Guid Id { get; set; }
        public int Num { get; set; }
    }
}
