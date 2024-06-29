using AutoFilterer.Extensions;
using IPMS.Business.Common.Constants;
using IPMS.Business.Common.Singleton;
using IPMS.Business.Common.Utils;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Models;
using IPMS.Business.Requests.Class;
using IPMS.Business.Responses.Class;
using IPMS.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IPMS.Business.Services
{
    public class ClassService : IClassService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IPMSUser> _userManager;
        public ClassService(IUnitOfWork unitOfWork, UserManager<IPMSUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        public async Task<ValidationResultModel> CheckSetMaxMemberRequestValid(Guid lecturerId, SetMaxMemberRequest request)
        {
            var result = new ValidationResultModel
            {
                Message = "Cannot Set Max Member"
            };
            if (!request.ClassIds.Any())
            {
                result.Message = "Must have at least on class in request";
                return result;
            }
            var now = DateTime.Now;
            var currentSemester = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester;
            var minAccepptedStartDate = currentSemester.StartDate;
            var requestClasses = await _unitOfWork.IPMSClassRepository.Get().Include(c => c.Semester)
                                                                        .Where
                                                                        (
                                                                            //Check Start Date of class is in Current Semester or in the future
                                                                            c => request.ClassIds.Contains(c.Id) &&
                                                                            c.Semester!.StartDate.Date >= minAccepptedStartDate &&
                                                                            //Check Change Group Deadline still in the future
                                                                            c.ChangeGroupDeadline > now &&
                                                                            //Check lecturer is teaching all class in request or not
                                                                            c.LecturerId == lecturerId
                                                                        )
                                                                        .CountAsync();
            if (requestClasses < request.ClassIds.Count())
            {
                result.Message = "Request contains class cannot modify max member or class is not existed";
                return result;
            }
            //Check all existing group have number of member less than request
            var isExistGreaterGroup = await _unitOfWork.StudentRepository.Get().Where
                                                                                (
                                                                                    stu => request.ClassIds.Contains(stu.ClassId) &&
                                                                                    stu.ProjectId != null
                                                                                )
                                                                                .GroupBy(x => new { x.ProjectId, x.ClassId })
                                                                                .AnyAsync
                                                                                (
                                                                                    group => group.Count() > request.MaxMember
                                                                                );
            if (isExistGreaterGroup)
            {
                result.Message = "At least one group have more member that max member you want";
                return result;
            }
            result.Result = true;
            result.Message = string.Empty;
            return result;
        }

        public async Task SetMaxMember(Guid lecturerId, SetMaxMemberRequest request)
        {
            var updateClasses = request.ClassIds.Select(x => new IPMSClass
            {
                Id = x
            });
            foreach (var c in updateClasses)
            {
                _unitOfWork.IPMSClassRepository.Attach(c);
                c.MaxMember = request.MaxMember;
            }
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task<IList<ClassGroupResponse>> GetGroupsInClass(Guid classId)
        {
            var result =  await _unitOfWork.ProjectRepository.Get().Include(x=>x.Students).Where(x=>x.Students.First().ClassId == classId)
                                                                          .Select(x=> new ClassGroupResponse
                                                                          {
                                                                              Id= x.Id,
                                                                              Name= x.GroupName
                                                                          }).ToListAsync();
            result.Add(new ClassGroupResponse()
            {
                Id = NoGroup.Id,
                Name = NoGroup.Name,
            });
            return result;
        }
        public async Task<MemberInGroupResponse> GetMemberInGroupAsync(MemberInGroupRequest request)
        {
            var memberInfos = _userManager.Users.ApplyFilter(request);
            if(request.GroupFilter != null && request.GroupFilter.Any())
            {
                // If not contain No Group filter
                if (!request.GroupFilter.Remove(NoGroup.Id))
                {
                    memberInfos = memberInfos.Include(x => x.Students.Where(stu => stu.ProjectId != null && request.GroupFilter.Contains(stu.ProjectId.Value))).ThenInclude(x => x.Project);
                    memberInfos = memberInfos.Where(x => x.Students.First().ProjectId != null && request.GroupFilter.Contains(x.Students.First().ProjectId.Value));
                }
                else
                {
                    memberInfos = memberInfos.Include(x => x.Students.Where(stu =>stu.ProjectId == null || request.GroupFilter.Contains(stu.ProjectId.Value))).ThenInclude(x => x.Project);
                    memberInfos = memberInfos.Where(x => x.Students.First().ProjectId == null || request.GroupFilter.Contains(x.Students.First().ProjectId.Value));
                }
            }
            
            return new MemberInGroupResponse()
            {
                TotalMember =  await _unitOfWork.StudentRepository.Get().CountAsync(x=>x.ClassId == request.Students.ClassId),
                MemberInfo = memberInfos.Select(x=>new MemberInGroupData
                {
                    Id = x.Id,
                    GroupName = x.Students.First().ProjectId != null ? x.Students.First().Project.GroupName : NoGroup.Name,
                    StudentId = x.UserName,
                    StudentName = x.FullName
                })
            };
        }
    }
}
