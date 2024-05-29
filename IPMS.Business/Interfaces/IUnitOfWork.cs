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
        IComponentsMasterRepository ComponentsMasterRepository { get; }
        IIoTComponentRepository IoTComponentRepository { get; }
        void SaveChangesAsync();
    }
}
