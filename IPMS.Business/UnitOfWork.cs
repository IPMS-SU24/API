using IPMS.Business.Interfaces;
using IPMS.Business.Repository;
using IPMS.DataAccess;

namespace IPMS.Business
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private IPMSDbContext _context;

        //TODO in Sprint 2
        //Add repository
        //Waiting for generate entities
        public UnitOfWork(IPMSDbContext context)
        {
            _context = context;
        }
        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }    }
}
