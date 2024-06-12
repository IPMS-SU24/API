using IPMS.Business.Models;
using IPMS.Business.Requests.Group;
using IPMS.Business.Responses.Group;

namespace IPMS.Business.Interfaces.Services
{
    public interface IStudentGroupService
    {
        Task<StudentGroupResponse> GetStudentGroupInformation(Guid studentId);
        Task<CreateGroupResponse> CreateGroup(CreateGroupRequest request, Guid studentId);
        Task<(Guid GroupId, Guid? MemberForSwapId)?> GetRequestGroupModel(Guid studentId);
        Task<ValidationResultModel> CheckStudentValidForCreateGroup(Guid studentId);
        Task<ValidationResultModel> CheckValidRequestSwap(SwapGroupRequest request, Guid studentId);
        Task<ValidationResultModel> CheckValidRequestJoin(JoinGroupRequest request, Guid studentId);
        Task<ValidationResultModel> CheckValidAssignLeaderRequest(AssignLeaderRequest request, Guid studentId);
        Task RequestToSwapGroup(SwapGroupRequest request, Guid studentId);
        Task RequestToJoinGroup(JoinGroupRequest request, Guid studentId);
        Task AssignLeader(AssignLeaderRequest request, Guid studentId);
    }
}
