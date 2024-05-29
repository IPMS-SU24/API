using IPMS.DataAccess.Common.Models;

namespace IPMS.DataAccess.Models
{
    public class Topic : BaseModel
    {
        public string? ShortName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string? Detail { get; set; }
        public Guid? OwnerId { get; set; }
        public virtual IPMSUser? Owner { get; set; }
        public virtual ICollection<TopicFavorite> Favorites { get; set; } = new List<TopicFavorite>();
        public virtual ICollection<ClassTopic> Classes { get; set; } = new List<ClassTopic>();

    }
}
