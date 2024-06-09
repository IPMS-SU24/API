﻿using IPMS.Business.Models;
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
    }
}
