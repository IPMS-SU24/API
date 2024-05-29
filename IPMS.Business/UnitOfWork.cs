using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Repositories;
using IPMS.DataAccess;

namespace IPMS.Business
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly IPMSDbContext _context;
        public ISemesterRepository SemesterRepository { get; }
        public IClassTopicRepository ClassTopicRepository { get; }
        public ITopicRepository TopicRepository { get; }
        public IStudentRepository StudentRepository { get; }
        public IIPMSClassRepository IPMSClassRepository { get; }
        public IComponentsMasterRepository ComponentsMasterRepository { get; }
        public IIoTComponentRepository IoTComponentRepository { get; }
        //TODO in Sprint 2
        //Add repository
        //Waiting for generate entities
        public UnitOfWork(IPMSDbContext context, ISemesterRepository semesterRepository, ITopicRepository topicRepository, 
                    IClassTopicRepository classTopicRepository, 
                    IStudentRepository studentRepository, IIPMSClassRepository iPMSClassRepository,
                    IComponentsMasterRepository componentsMasterRepository, IIoTComponentRepository ioTComponentRepository)
        {
            _context = context;
            SemesterRepository = semesterRepository;
            TopicRepository = topicRepository;
            ClassTopicRepository = classTopicRepository;
            StudentRepository = studentRepository;
            IPMSClassRepository = iPMSClassRepository;
            ComponentsMasterRepository = componentsMasterRepository;
            IoTComponentRepository = ioTComponentRepository;
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
