using IPMS.Business.Models;
using IPMS.Business.Requests.MemberHistory;
using IPMS.Business.Responses.MemberHistory;

namespace IPMS.Business.Interfaces.Services
{
    public interface IMemberHistoryService
    {
        Task<List<LoggedInUserHistoryResponse>> GetLoggedInUserHistories(Guid currentUserId);
        Task<ValidationResultModel> UpdateRequestStatusValidators(UpdateRequestStatusRequest request, Guid studentId);
        Task UpdateRequestStatus(UpdateRequestStatusRequest request, Guid currentUserId);
    }
}
