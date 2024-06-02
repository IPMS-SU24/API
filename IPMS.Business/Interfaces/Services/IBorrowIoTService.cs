using IPMS.Business.Requests;
using IPMS.DataAccess.Models;

namespace IPMS.Business.Interfaces.Services
{
    public interface IBorrowIoTService
    {
        Task<bool> CheckIoTValid(BorrowIoTModelRequest request, Guid leaderId);
        Task RegisterIoTForProject(Guid leaderId, IEnumerable<BorrowIoTModelRequest> borrowIoTModels);
        Task GetAvailableIoTComponents(Guid leaderId, IEnumerable<BorrowIoTModelRequest> borrowIoTModels);
    }
}
