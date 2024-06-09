using IPMS.Business.Common.Exceptions;
using IPMS.Business.Common.Utils;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Responses.Group;
using IPMS.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using IPMS.DataAccess.Common.Enums;
using Microsoft.EntityFrameworkCore;

namespace IPMS.Business.Services
{
    public class StudentGroupService : IStudentGroupService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonServices _commonServices;
        private readonly UserManager<IPMSUser> _userManager;
        public StudentGroupService(IUnitOfWork unitOfWork, ICommonServices commonServices, UserManager<IPMSUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _commonServices = commonServices;
            _userManager = userManager;
        }

        public async Task<(Guid GroupId, Guid? MemberForSwapId)?> GetRequestGroupModel(Guid studentId)
        {
            var currentRequest = await _unitOfWork.MemberHistoryRepository.Get().Where(x => x.ReporterId == studentId
                                                                                && (x.ProjectFromStatus == RequestStatus.Waiting 
                                                                                || x.ProjectToStatus == RequestStatus.Waiting
                                                                                || x.MemberSwapStatus == RequestStatus.Waiting)).FirstOrDefaultAsync();
            return currentRequest == null ? null : new()
            {
                GroupId = currentRequest.ProjectToId!.Value,
                MemberForSwapId = currentRequest.MemberSwapId,
            };
        }

        public async Task<StudentGroupResponse> GetStudentGroupInformation(Guid studentId)
        {
            var studiesIn = await _commonServices.GetStudiesIn(studentId);
            if (studiesIn.IsNullOrEmpty()) throw new DataNotFoundException();
            var currentSemester = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester;
            var @class = await _commonServices.GetCurrentClass(studiesIn.Select(x => x.ClassId), currentSemester!.Id);
            if (@class == null) throw new DataNotFoundException();
            var lecturer = await _userManager.FindByIdAsync(@class.LecturerId.ToString());
            //Get Join/ Swap Group Request
            var requestGroupModel = await GetRequestGroupModel(studentId);
            var leaderList = (await _userManager.GetUsersInRoleAsync("Leader")).Select(x => x.Id).ToList();
            //Get Groups
            var groupsInClass = await _unitOfWork.StudentRepository.Get().Where(x => x.ClassId == @class.Id && x.ProjectId != null).Include(x => x.Project).GroupBy(x => new { x.Project!.Id, x.Project!.GroupName })

                //    _unitOfWork.ClassTopicRepository.Get().Where(x => x.ClassId == @class.Id && x.ProjectId != null).Include(x => x.Project)
                //    .Select(x => new
                //    {
                //        ProjectId = x.Project!.Id,
                //        x.Project.GroupName
                //    })
                //    .Join(_unitOfWork.StudentRepository.Get().Include(x => x.Information),
                //            project => project.ProjectId,
                //            student => student.ProjectId,
                //            (project, student) => new
                //            {
                //                project.ProjectId,
                //                project.GroupName,
                //                StudentId = student.Id,
                //                Name = student.Information!.FullName
                //            }).ToList();
                //var 


                //.SelectMany(x => x.ToList(),
                //(project, student) => new
                //{
                //    project.Key.Id,
                //    project.Key.GroupName,
                //    StudentId = student.InformationId,
                //    IsLeader = leaderList.Contains(student.InformationId!.Value),
                //    Name = student.Information!.FullName
                //}).ToList();
                .Select(x => new ClassGroupInfo
                {
                    Id = x.Key.Id,
                    GroupName = x.Key.GroupName,
                    Members = x.Select(y => new GroupMemberInfo
                    {
                        Id = y.InformationId,

                        IsLeader = leaderList.Contains(y.InformationId),
                        Name = y.Information.FullName
                    }).ToList(),
                    IsYourGroup = x.Any(y=>y.InformationId == studentId),
                }).ToListAsync();
            return new()
            {
                Class = new()
                {
                    ClassCode = @class.Name,
                    MaxMembers = @class.MaxMember,
                    Semester = currentSemester.ShortName
                },
                Lecturer = new()
                {
                    Email = lecturer.Email,
                    Name = lecturer.FullName,
                    Phone = lecturer.PhoneNumber
                },
                Groups = groupsInClass,
                GroupJoinRequest = requestGroupModel?.GroupId,
                MemberSwapRequest = requestGroupModel?.MemberForSwapId
            };
        }
    }
}
