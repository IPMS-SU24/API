using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Repositories;
using IPMS.Business.Repository;
using IPMS.DataAccess;

namespace IPMS.Business
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly IPMSDbContext _context;
        public ISemesterRepository SemesterRepository { get; }
        public ITopicRepository TopicRepository { get; }
        //TODO in Sprint 2
        //Add repository
        //Waiting for generate entities
        public UnitOfWork(IPMSDbContext context, ISemesterRepository semesterRepository, ITopicRepository topicRepository)
        {
            _context = context;
            SemesterRepository = semesterRepository;
            TopicRepository = topicRepository;
        }
        public void SaveChangesAsync()
        {
            _context.SaveChangesAsync();
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
