using IPMS.Business.Models;
using IPMS.Business.Requests.MemberHistory;
using IPMS.Business.Responses.MemberHistory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPMS.Business.Interfaces.Services
{
    public interface IMemberHistoryService
    {
        Task<List<LoggedInUserHistoryResponse>> GetLoggedInUserHistories(Guid currentUserId);
        Task<ValidationResultModel> UpdateRequestStatusValidators(UpdateRequestStatusRequest request, Guid studentId);
        Task UpdateRequestStatus(UpdateRequestStatusRequest request, Guid currentUserId);
    }
}
