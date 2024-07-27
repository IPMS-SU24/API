using IPMS.Business.Models;
using IPMS.Business.Requests.Class;
using IPMS.Business.Responses.Class;
using IPMS.DataAccess.Models;

namespace IPMS.Business.Interfaces.Services
{
    public interface IClassService
    {
        Task SetMaxMember(Guid lecturerId, SetMaxMemberRequest request);
        Task<ValidationResultModel> CheckSetMaxMemberRequestValid(Guid lecturerId, SetMaxMemberRequest request);
        Task<IList<ClassGroupResponse>> GetGroupsInClass(Guid classId);
        Task<MemberInGroupResponse> GetMemberInGroupAsync(MemberInGroupRequest request);
        Task AddStudentAsync(AddStudentsToClassRequest request);
        Task<ValidationResultModel> CheckImportStudentValidAsync(AddStudentsToClassRequest request, Guid lecturerId);
        Task<JobImportStatusResponse> GetImportStudentStatusAsync(Guid classId);
        Task RemoveOutOfClassAsync(RemoveOutOfClassRequest request);
        Task<ValidationResultModel> CheckValidRemoveOutOfClass(RemoveOutOfClassRequest request, Guid lecturerId);
        Task<GetClassDetailResponse> GetClassDetail(Guid classId);
        Task<ValidationResultModel> UpdateClassDetailValidators(UpdateClassDetailRequest request);
        Task UpdateClassDetail(UpdateClassDetailRequest request);
    }
}
