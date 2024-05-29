using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Repositories;
using IPMS.Business.Repository;
using IPMS.DataAccess;

namespace IPMS.Business
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly IPMSDbContext _context;

        public IProjectRepository ProjectRepository { get; }

        public IStudentRepository StudentRepository { get; }

        public IClassTopicRepository ClassTopicRepository {  get; }

        public IProjectSubmissionRepository ProjectSubmissionRepository { get; }
        public ISubmissionModuleRepository SubmissionModuleRepository { get; }
        public IIPMSClassRepository IPMSClassRepository { get; }

        public ISemesterRepository SemesterRepository { get; }

        public ISyllabusRepository SyllabusRepository { get; }

        public IAssessmentRepository AssessmentRepository { get; }

        public ITopicRepository TopicRepository { get; }

        public UnitOfWork(IPMSDbContext context,
            IProjectRepository projectRepository,
            IStudentRepository studentRepository,
            IClassTopicRepository classTopicRepository,
            ISubmissionModuleRepository submissionModuleRepository,
            IIPMSClassRepository iPMSClassRepository,
            ISemesterRepository semesterRepository,
            ISyllabusRepository syllabusRepository,
            IAssessmentRepository assessmentRepository,
            ITopicRepository topicRepository,
            IProjectSubmissionRepository projectSubmissionRepository)
        {
            _context = context;
            ProjectRepository = projectRepository;
            StudentRepository = studentRepository;
            ClassTopicRepository = classTopicRepository;
            SubmissionModuleRepository = submissionModuleRepository;
            IPMSClassRepository = iPMSClassRepository;
            SemesterRepository = semesterRepository;
            SyllabusRepository = syllabusRepository;
            AssessmentRepository = assessmentRepository;
            TopicRepository = topicRepository;
            ProjectSubmissionRepository = projectSubmissionRepository;
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
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
        }
    }
}
