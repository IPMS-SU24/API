
namespace IPMS.Business.Requests.Topic
{
    public class ReviewSuggestedTopicRequest
    {
        public Guid ClassId { get; set; }
        public Guid TopicId { get; set; }
        public bool IsApproved { get; set; } = false;
    }
}
