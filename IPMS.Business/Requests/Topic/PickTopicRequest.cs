﻿namespace IPMS.Business.Requests.Topic
{
    public class PickTopicRequest
    {
        public Guid TopicId { get; set; }
        public Guid? AssessmentId { get; set; }
    }
}
