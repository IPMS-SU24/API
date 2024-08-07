namespace IPMS.Business.Requests.ClassTopic
{
    public class LecturerClassTopicRequest
    {
        public Guid ClassId { get; set; }
        public bool? IsCommittee { get; set; }
    }
}
