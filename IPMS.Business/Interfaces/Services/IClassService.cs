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
        Task<JobImportStatusResponse<JobImportStudentStatusRecord>?> GetImportStudentStatusAsync(string classCode);
        Task RemoveOutOfClassAsync(RemoveOutOfClassRequest request);
        Task<ValidationResultModel> CheckValidRemoveOutOfClass(RemoveOutOfClassRequest request, Guid lecturerId);
        Task<GetClassDetailResponse> GetClassDetail(Guid classId);
        Task<ValidationResultModel> UpdateClassDetailValidators(UpdateClassDetailRequest request);
        Task AddClassesAsync(ImportClassRequest request);
        Task UpdateClassDetail(UpdateClassDetailRequest request);
        Task<bool> IsClassCodeExistInSemesterAsync(string classCode, Guid semesterId);
        Task<IEnumerable<GetClassDetailResponse>> GetClassList(GetClassListRequest request);
        Task<JobImportStatusResponse<JobImportClassStatusRecord>?> GetImportClassStatusAsync(Guid semesterId);
        Task<ValidationResultModel> CheckImportClassValidAsync(ImportClassRequest request);
        Task<ValidationResultModel> UpdateClassDeadlineValidators(UpdateClassDeadlineRequest request, Guid lecturerId);
        Task UpdateClassDeadline(UpdateClassDeadlineRequest request, Guid lecturerId);
        Task<GetClassDeadlineResponse> GetClassDeadline(Guid classId, Guid lecturerId);
        Task<ClassGradeExportResponse> ExportGradesAsync(ClassExportGradeRequest request);
        Task<ValidationResultModel> CheckExportGradeValid(ClassExportGradeRequest x, Guid lecturerId);
        Task<IList<ClassGradeDataRow>> GetClassGrades(ClassExportGradeRequest request);
    }
}
