using IPMS.Business.Models;
using IPMS.Business.Requests.Class;
using IPMS.Business.Responses.Class;

namespace IPMS.Business.Interfaces.Services
{
    public interface IClassService
    {
        Task SetMaxMember(Guid lecturerId, SetMaxMemberRequest request);
        Task<ValidationResultModel> CheckSetMaxMemberRequestValid(Guid lecturerId, SetMaxMemberRequest request);
        Task<IList<ClassGroupResponse>> GetGroupsInClass(Guid classId);
        Task<MemberInGroupResponse> GetMemberInGroupAsync(MemberInGroupRequest request);
        Task AddStudentAsync(AddStudentsToClassRequest request);
        Task<string> GetImportStudentStatusAsync(Guid classId);
        Task<ValidationResultModel> CheckImportStudentValidAsync(AddStudentsToClassRequest request, Guid lecturerId);
    }
}
