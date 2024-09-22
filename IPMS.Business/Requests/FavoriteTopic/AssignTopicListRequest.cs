
namespace IPMS.Business.Requests.FavoriteTopic
{
    public class AssignTopicListRequest
    {
        public List<Guid> ClassesId { get; set; } = new();
        public Guid ListId { get; set; }

        public Guid? AssessmentId { get; set; }
    }
}
