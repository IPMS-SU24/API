using IPMS.Business.Interfaces.Repositories;
using IPMS.Business.Repository;
using IPMS.DataAccess.Models;
using IPMS.DataAccess;
using IPMS.DataAccess.Common.Enums;

namespace IPMS.Business.Repositories
{
    public class ComponentsMasterRepository : GenericRepository<ComponentsMaster>, IComponentsMasterRepository
    {
        public ComponentsMasterRepository(IPMSDbContext context) : base(context)
        {

        }

        public IQueryable<ComponentsMaster> GetBorrowComponents()
        {
            return base.Get().Where(x => x.MasterType == ComponentsMasterType.Project);
        }

        public IQueryable<ComponentsMaster> GetLecturerOwnComponents()
        {
            return base.Get().Where(x => x.MasterType == ComponentsMasterType.Lecturer && x.Status == null);
        }

        public IQueryable<ComponentsMaster> GetTopicComponents()
        {
            return base.Get().Where(x => x.MasterType == ComponentsMasterType.Topic && x.Status == null);
        }
    }
}
