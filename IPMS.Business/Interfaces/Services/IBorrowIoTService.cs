using IPMS.Business.Models;
using IPMS.Business.Requests.IoTComponent;
using IPMS.Business.Responses.IoT;
using IPMS.Business.Responses.ProjectDashboard;

namespace IPMS.Business.Interfaces.Services
{
    public interface IBorrowIoTService
    {
        Task<ValidationResultModel> CheckIoTValid(IoTModelRequest request, Guid leaderId);
        Task RegisterIoTForProject(Guid leaderId, IEnumerable<IoTModelRequest> borrowIoTModels);
        Task<IEnumerable<BorrowIoTComponentInformation>> GetAvailableIoTComponents(GetAvailableComponentRequest request, Guid leaderId);
        Task<IEnumerable<ReportIoTComponentInformation>> GetGetReportIoTComponents();
        Task<IEnumerable<GetBorrowIoTComponentsResponse>> GetBorrowIoTComponents(GetBorrowIoTComponentsRequest request, Guid lecturerId);
        Task<ValidationResultModel> ReviewBorrowIoTComponentsValidators(ReviewBorrowIoTComponentsRequest request, Guid lecturerId);
        Task ReviewBorrowIoTComponents(ReviewBorrowIoTComponentsRequest request, Guid lecturerId);

        Task<ValidationResultModel> ReturnIoTComponentsValidators(ReturnIoTComponentsRequest request, Guid lecturerId);
        Task ReturnIoTComponents(ReturnIoTComponentsRequest request, Guid lecturerId);
    }
}
