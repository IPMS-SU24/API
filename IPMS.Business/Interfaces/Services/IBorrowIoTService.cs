using IPMS.Business.Requests.IoTComponent;
using IPMS.Business.Responses.ProjectDashboard;
using IPMS.DataAccess.Models;

namespace IPMS.Business.Interfaces.Services
{
    public interface IBorrowIoTService
    {
        Task<bool> CheckIoTValid(IoTModelRequest request, Guid leaderId);
        Task RegisterIoTForProject(Guid leaderId, IEnumerable<IoTModelRequest> borrowIoTModels);
        Task<IEnumerable<BorrowIoTComponentInformation>> GetAvailableIoTComponents(GetAvailableComponentRequest request, Guid leaderId);
    }
}
