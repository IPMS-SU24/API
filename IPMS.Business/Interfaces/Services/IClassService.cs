using IPMS.Business.Models;
using IPMS.Business.Requests.Class;

namespace IPMS.Business.Interfaces.Services
{
    public interface IClassService
    {
        Task SetMaxMember(Guid lecturerId, SetMaxMemberRequest request);
        Task<ValidationResultModel> CheckSetMaxMemberRequestValid(Guid lecturerId, SetMaxMemberRequest request);
    }
}
