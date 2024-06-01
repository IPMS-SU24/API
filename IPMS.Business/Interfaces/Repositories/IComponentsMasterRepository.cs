using IPMS.DataAccess.Models;

namespace IPMS.Business.Interfaces.Repositories
{
    public interface IComponentsMasterRepository : IGenericRepository<ComponentsMaster>
    {
        IQueryable<ComponentsMaster> GetLecturerOwnComponents();
        IQueryable<ComponentsMaster> GetTopicComponents();
        IQueryable<ComponentsMaster> GetBorrowComponents();
    }
}
