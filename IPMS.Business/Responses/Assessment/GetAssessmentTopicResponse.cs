namespace IPMS.Business.Responses.Assessment
{
    public class GetAssessmentTopicResponse
    {
        public Guid? TopicId { get; set; }
        public string? TopicName { get; set; }
        public string? TopicShortName { get; set; }
        public string? TopicDetailLink { get; set; }
        public string TopicDescription { get; set; }

        public string AssessmentName { get; set; } = null!;
        public Guid AssessmentId { get; set; }
    }
}
