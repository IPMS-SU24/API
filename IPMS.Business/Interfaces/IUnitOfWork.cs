using IPMS.Business.Interfaces.Repositories;

namespace IPMS.Business.Interfaces
{
    public interface IUnitOfWork
    {
        ISemesterRepository SemesterRepository { get; }
        void SaveChangesAsync();
    }
}
