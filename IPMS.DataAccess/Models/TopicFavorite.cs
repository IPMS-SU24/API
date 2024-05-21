using IPMS.DataAccess.Common.Models;

namespace IPMS.DataAccess.Models
{
    public class TopicFavorite : BaseModel
    {
        public Guid? TopicId { get; set; }
        public virtual Topic? Topic { get; set; }
        public Guid? FavoriteId { get; set; }
        public virtual Favorite? Favorite { get; set; }
    }
}
