using Microsoft.AspNetCore.Mvc;

namespace IPMS.Business.Requests.Topic
{
    public class ChangeVisibleTopicRequest
    {
        public Guid Id { get; set; }
        public bool IsPublic { get; set; }
    }
}
