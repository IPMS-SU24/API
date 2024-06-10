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
using FluentValidation.Results;
using IPMS.Business.Models;
using IPMS.Business.Requests.Group;
using IPMS.Business.Common.Enums;

namespace IPMS.Business.Services
{
    public class StudentGroupService : IStudentGroupService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonServices _commonServices;
        private readonly UserManager<IPMSUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        public StudentGroupService(IUnitOfWork unitOfWork, ICommonServices commonServices, UserManager<IPMSUser> userManager, RoleManager<IdentityRole<Guid>> roleManager)
        {
            _unitOfWork = unitOfWork;
            _commonServices = commonServices;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<ValidationResultModel> CheckStudentValidForCreateGroup(Guid studentId)
        {
            var result = new ValidationResultModel
            {
                Message = "Student is not valid"
            };
            //Check student in any project
            var project = await _commonServices.GetProject(studentId);
            if(project != null)
            {
                result.Message = "Student is in a project";
                return result;
            }
            //Check student is studying
            var studiesIn = await _commonServices.GetStudiesIn(studentId);
            var currentClass = await _commonServices.GetCurrentClass(studiesIn.Select(x => x.ClassId));
            if(currentClass == null)
            {
                result.Message = "Student is not studying";
                return result;
            }
            result.Message = string.Empty;
            result.Result = true;
            return result;
        }

        public async Task<CreateGroupResponse> CreateGroup(CreateGroupRequest request, Guid studentId)
        {
            //Get Student
            var studiesIn = await _commonServices.GetStudiesIn(studentId);
            var currentClass = await _commonServices.GetCurrentClass(studiesIn.Select(x => x.ClassId));
            var student = studiesIn.Where(x=>x.ClassId == currentClass.Id).FirstOrDefault();
            var project = new Project
            {
                GroupName = request.GroupName,
                OwnerId = currentClass.LecturerId
            };
            await _unitOfWork.ProjectRepository.InsertAsync(project);
            student.ProjectId = project.Id;
            //Update Student
            _unitOfWork.StudentRepository.Update(student);
            var studentAccount = await _userManager.FindByIdAsync(studentId.ToString());
            if (!await _roleManager.RoleExistsAsync(UserRole.Leader.ToString())) await _roleManager.CreateAsync(new IdentityRole<Guid>(UserRole.Leader.ToString()));
            await _userManager.AddToRoleAsync(studentAccount, UserRole.Leader.ToString());
            await _unitOfWork.SaveChangesAsync();
            return new()
            {
                ProjectId = student.ProjectId.Value
            };
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
