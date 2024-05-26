using IPMS.Business.Interfaces.Repositories;

namespace IPMS.Business.Interfaces
{
    public interface IUnitOfWork
    {
        ISemesterRepository SemesterRepository { get; }
        ITopicRepository TopicRepository { get; }
        IClassTopicRepository ClassTopicRepository { get; }
        IStudentRepository StudentRepository { get; }
        IIPMSClassRepository IPMSClassRepository { get; }
        void SaveChangesAsync();
    }
}
