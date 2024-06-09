using IPMS.Business.Responses.Group;

namespace IPMS.Business.Interfaces.Services
{
    public interface IStudentGroupService
    {
        Task<StudentGroupResponse> GetStudentGroupInformation(Guid studentId);
        Task<(Guid GroupId, Guid? MemberForSwapId)?> GetRequestGroupModel(Guid studentId);
    }
}
