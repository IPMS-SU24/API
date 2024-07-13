﻿namespace IPMS.Business.Responses.FavoriteTopic
{
    public class GetFavoriteTopicResponse
    {
        public Guid TopicId { get; set; }
        public string TopicName { get; set; }
        public string Description { get; set; }
        public List<FavoriteIoTInfo> IoTComponents { get; set; } = new List<FavoriteIoTInfo>();
        public string? DetailLink { get; set; }
        public bool IsBelongToList { get; set; }
    }

    public class FavoriteIoTInfo
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid Id { get; set; }
    }
}
