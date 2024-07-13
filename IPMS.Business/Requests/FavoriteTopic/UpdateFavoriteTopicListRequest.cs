namespace IPMS.Business.Requests.FavoriteTopic
{
    public class UpdateFavoriteTopicListRequest
    {
        public Guid ListId { get; set; }
        public IList<Guid> TopicIds { get; set; } = new List<Guid>();
    }
}
