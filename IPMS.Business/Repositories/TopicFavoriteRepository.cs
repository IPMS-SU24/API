using IPMS.Business.Interfaces.Repositories;
using IPMS.Business.Repository;
using IPMS.DataAccess;
using IPMS.DataAccess.Models;

namespace IPMS.Business.Repositories
{
    public class TopicFavoriteRepository : GenericRepository<TopicFavorite>, ITopicFavoriteRepository
    {
        public TopicFavoriteRepository(IPMSDbContext context) : base(context)
        {
        }
    }
}
