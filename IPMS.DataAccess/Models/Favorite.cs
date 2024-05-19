using System;
using IPMS.DataAccess.CommonModels;

namespace IPMS.DataAccess.Models
{
    public class Favorite : BaseModel
    {
        public string Name { get; set; }
        public Guid LecturerId { get; set; }
        public virtual IPMSUser Lecturer { get; set; }
        public virtual ICollection<TopicFavorite> TopicFavorites { get; set; } = new List<TopicFavorite>();
        
    }
}
