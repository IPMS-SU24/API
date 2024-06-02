using IPMS.Business.Requests.IoTComponent;
using IPMS.Business.Responses.ProjectDashboard;
using IPMS.DataAccess.Models;

namespace IPMS.Business.Interfaces.Services
{
    public interface IBorrowIoTService
    {
        Task<bool> CheckIoTValid(BorrowIoTModelRequest request, Guid leaderId);
        Task RegisterIoTForProject(Guid leaderId, IEnumerable<BorrowIoTModelRequest> borrowIoTModels);
        Task<IEnumerable<BorrowIoTComponentInformation>> GetAvailableIoTComponents(GetAvailableComponentRequest request, Guid leaderId);
    }
}
