using IPMS.Business.Models;
using IPMS.Business.Requests.Group;
using IPMS.Business.Responses.Group;

namespace IPMS.Business.Interfaces.Services
{
    public interface IStudentGroupService
    {
        Task<StudentGroupResponse> GetStudentGroupInformation(Guid studentId);
        Task<CreateGroupResponse> CreateGroup(Guid studentId);
        Task<(Guid GroupId, Guid? MemberForSwapId)?> GetRequestGroupModel(Guid studentId);
        Task<ValidationResultModel> CheckStudentValidForCreateGroup(Guid studentId);
        Task<ValidationResultModel> CheckValidRequestSwap(SwapGroupRequest request, Guid studentId);
        Task<ValidationResultModel> CheckValidRequestJoin(JoinGroupRequest request, Guid studentId);
        Task<ValidationResultModel> CheckValidAssignLeaderRequest(AssignLeaderRequest request, Guid studentId);
        Task RequestToSwapGroup(SwapGroupRequest request, Guid studentId);
        Task RequestToJoinGroup(JoinGroupRequest request, Guid studentId);
        Task AssignLeader(AssignLeaderRequest request, Guid studentId);
        Task<ValidationResultModel> RemoveStudentOutGroupValidators(RemoveStudentOutGroupRequest request, Guid lecturerId);
        Task RemoveStudentOutGroup(RemoveStudentOutGroupRequest request, Guid lecturerId);
        Task<ValidationResultModel> AddMemberValidators(Guid studentId, Guid projectId);
        Task AddMember(Guid studentId, Guid projectId);
        Task AddStudentsToGroup(LecturerAddStudentsToGroupRequest request, Guid lecturerId);
        Task<ValidationResultModel> CheckValidForLecturerAddStudentToGroup(LecturerAddStudentsToGroupRequest request, Guid lecturerId);
        Task EvaluateMembers(LeaderEvaluateMembersRequest request, Guid leaderId);
        Task<ValidationResultModel> CheckValidEvaluateMembers(IList<MemberContribute> x);
        Task<IList<MemberEvaluateResponse>> GetEvaluateMembers(Guid studentId);
        Task<IList<MemberEvaluateResponse>> GetEvaluateMembersByLecturer(GetMemberContributionRequest request);
        Task<ValidationResultModel> CheckGetContributeByLecturer(GetMemberContributionRequest request, Guid lecturerId);
    }
}
