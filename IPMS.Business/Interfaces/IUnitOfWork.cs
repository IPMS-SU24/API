using IPMS.Business.Interfaces.Repositories;

namespace IPMS.Business.Interfaces
{
    public interface IUnitOfWork
    {
        IProjectRepository ProjectRepository { get; }
        IStudentRepository StudentRepository { get; }
        IClassTopicRepository ClassTopicRepository { get; }
        IProjectSubmissionRepository ProjectSubmissionRepository { get; }
        ISubmissionModuleRepository SubmissionModuleRepository { get; }
        IIPMSClassRepository IPMSClassRepository { get; }
        ISemesterRepository SemesterRepository { get; }
        ISyllabusRepository SyllabusRepository { get; }
        IAssessmentRepository AssessmentRepository { get; }
        ITopicRepository TopicRepository { get; }
        IReportTypeRepository ReportTypeRepository { get; }
        IReportRepository ReportRepository { get; }
        IMemberHistoryRepository MemberHistoryRepository { get; }
        ILecturerGradeRepository LecturerGradeRepository { get; }
        IComponentsMasterRepository ComponentsMasterRepository { get; }
        IIoTComponentRepository IoTComponentRepository { get; }
        IFavoriteRepository FavoriteRepository { get; }
        ITopicFavoriteRepository TopicFavoriteRepository { get; }
        //Start the database Transaction
        Task CreateTransactionAsync();
        //Commit the database Transaction
        Task CommitAsync();
        //Rollback the database Transaction
        Task RollbackAsync();
        Task SaveChangesAsync();
        void SaveChanges();
        Task RollbackTransactionOnFailAsync(Func<Task> resultBody);
    }
}
