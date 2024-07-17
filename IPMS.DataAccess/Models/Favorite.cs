using IPMS.DataAccess.Common.Models;

namespace IPMS.DataAccess.Models
{
    public class Favorite : BaseModel
    {
        public string Name { get; set; }
        public Guid LecturerId { get; set; }
        public virtual IPMSUser Lecturer { get; set; }
        public virtual ICollection<TopicFavorite> Topics { get; set; } = new List<TopicFavorite>();
        
    }
}
