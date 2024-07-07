using IPMS.Business.Common.Enums;
using IPMS.Business.Common.Exceptions;
using IPMS.Business.Common.Utils;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Models;
using IPMS.Business.Requests.Group;
using IPMS.Business.Responses.Group;
using IPMS.DataAccess.Common.Enums;
using IPMS.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;

namespace IPMS.Business.Services
{
    public class StudentGroupService : IStudentGroupService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonServices _commonServices;
        private readonly IMessageService _messageService;
        private readonly UserManager<IPMSUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        public StudentGroupService(IUnitOfWork unitOfWork, ICommonServices commonServices, UserManager<IPMSUser> userManager, RoleManager<IdentityRole<Guid>> roleManager, IMessageService messageService)
        {
            _unitOfWork = unitOfWork;
            _commonServices = commonServices;
            _userManager = userManager;
            _roleManager = roleManager;
            _messageService = messageService;
        }

        public async Task<ValidationResultModel> CheckStudentValidForCreateGroup(Guid studentId)
        {
            var result = new ValidationResultModel
            {
                Message = "Student is not valid"
            };
            //Check student in any project
            var project = await _commonServices.GetProject(studentId);
            if (project != null)
            {
                result.Message = "Student is in a project";
                return result;
            }
            //Check student is studying
            var studiesIn = await _commonServices.GetStudiesIn(studentId);
            var currentClass = await _commonServices.GetCurrentClass(studiesIn.Select(x => x.ClassId));
            if (currentClass == null)
            {
                result.Message = "Student is not studying";
                return result;
            }
            var projectInClassCount = await _unitOfWork.StudentRepository.Get().Where(x => x.ClassId == currentClass.Id).Select(x=>x.ProjectId).Distinct().CountAsync();
            var topicOfClassCount = await _unitOfWork.ClassTopicRepository.Get().Where(x => x.ClassId == currentClass.Id).CountAsync();
            if(projectInClassCount == topicOfClassCount)
            {
                result.Message = "Class cannot create more project";
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
            var student = studiesIn.Where(x => x.ClassId == currentClass.Id).FirstOrDefault();
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
            var leaderList = (await _userManager.GetUsersInRoleAsync(UserRole.Leader.ToString())).Select(x => x.Id).ToList();
            //Get Groups
            var groupsInClass = await _unitOfWork.StudentRepository.Get().Where(x => x.ClassId == @class.Id && x.ProjectId != null).Include(x => x.Project).GroupBy(x => new { x.Project!.Id, x.Project!.GroupName })
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
                                                                            IsYourGroup = x.Any(y => y.InformationId == studentId),
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

        public async Task RequestToSwapGroup(SwapGroupRequest request, Guid studentId)
        {
            var reporterProject = await _commonServices.GetProject(studentId);
            var memberSwapForProject = await _commonServices.GetProject(request.MemberId);
            await _unitOfWork.ProjectRepository.LoadExplicitProperty(reporterProject, nameof(Project.Students));
            await _unitOfWork.ProjectRepository.LoadExplicitProperty(memberSwapForProject, nameof(Project.Students));
            var leaderList = (await _userManager.GetUsersInRoleAsync(UserRole.Leader.ToString())).Select(x => x.Id).ToList();
            var memberHistory = new MemberHistory()
            {
                MemberSwapId = request.MemberId,
                ReporterId = studentId,
                ProjectFromId = reporterProject.Id,
                ProjectToId = memberSwapForProject.Id,
                IPMSClassId = _commonServices.GetClass()!.Id
            };
            await _unitOfWork.SaveChangesAsync();
            var sendMessageTasks = new List<Task>();
            //Add leaderId to notification
            foreach (var member in reporterProject.Students)
            {
                if (leaderList.Contains(member.InformationId))
                {
                    await _unitOfWork.StudentRepository.LoadExplicitProperty(member, nameof(Student.Information));
                    sendMessageTasks.Add(_messageService.SendMessage(new NotificationMessage
                    {
                        AccountId = member.InformationId,
                        Message = $"Member {member.Information.FullName} have been requested swap to group {memberSwapForProject.GroupName}",
                        Title = "Swap Group Request"
                    }));
                }
            }
            //Add leaderId to notification
            foreach (var member in memberSwapForProject.Students)
            {
                if (leaderList.Contains(member.InformationId))
                {
                    await _unitOfWork.StudentRepository.LoadExplicitProperty(member, nameof(Student.Information));
                    sendMessageTasks.Add(_messageService.SendMessage(new NotificationMessage
                    {
                        AccountId = member.InformationId,
                        Message = $"Member {member.Information.FullName} have been requested swap to your group",
                        Title = "Swap Group Request"
                    }));
                }
            }
            await _unitOfWork.MemberHistoryRepository.InsertAsync(memberHistory);
            sendMessageTasks.Add(_messageService.SendMessage(new NotificationMessage
            {
                AccountId = request.MemberId,
                Message = $"You are requested to swap to group {reporterProject.GroupName}",
                Title = "Swap Group Request"
            }));
        }

        public async Task<ValidationResultModel> CheckValidRequestSwap(SwapGroupRequest request, Guid studentId)
        {
            var result = new ValidationResultModel
            {
                Message = "Student cannot swap"
            };
            //Check student is not leader
            var user = await _userManager.FindByIdAsync(studentId.ToString());
            var isLeader = await _userManager.IsInRoleAsync(user, UserRole.Leader.ToString());
            if (isLeader)
            {
                result.Message = "Leader cannot swap";
                return result;
            }
            var studiesIn = await _commonServices.GetStudiesIn(studentId);
            var @class = await _commonServices.GetCurrentClass(studiesIn.Select(x => x.ClassId));
            if (@class == null)
            {
                result.Message = "Student not in class";
                return result;
            }
            if (@class.ChangeGroupDeadline < DateTime.Now)
            {
                result.Message = "Cannot change group at this time";
            }

            var reporterProject = await _commonServices.GetProject(studentId);
            if (reporterProject == null)
            {
                result.Message = "You are not in group";
                return result;
            }
            var memberForSwapProject = await _commonServices.GetProject(request.MemberId);
            if (memberForSwapProject == null)
            {
                result.Message = "Member For Swap not in any group";
                return result;
            }
            else
            {

                var swapStudiesIn = await _commonServices.GetStudiesIn(request.MemberId);
                var memberForSwapClass = await _commonServices.GetCurrentClass(swapStudiesIn.Select(x => x.ClassId));
                if (memberForSwapClass.Id != @class.Id)
                {
                    result.Message = "Member not in the same class";
                    return result;
                }
            }
            if (reporterProject.Id == memberForSwapProject.Id)
            {
                result.Message = "Cannot swap to the same project";
                return result;
            }
            result.Message = string.Empty;
            result.Result = true;
            return result;

        }

        public async Task<ValidationResultModel> CheckValidRequestJoin(JoinGroupRequest request, Guid studentId)
        {
            var result = new ValidationResultModel
            {
                Message = "Student cannot join"
            };
            //Check student is not leader
            var user = await _userManager.FindByIdAsync(studentId.ToString());

            var joinGroupExist = await _unitOfWork.ProjectRepository.Get().Where(x => x.Id == request.GroupId).Select(x => x.Id).FirstOrDefaultAsync();
            if (joinGroupExist == Guid.Empty)
            {
                result.Message = "Not found group";
                return result;
            }
            var studiesIn = await _commonServices.GetStudiesIn(studentId);
            var @class = await _commonServices.GetCurrentClass(studiesIn.Select(x => x.ClassId));
            if (@class == null)
            {
                result.Message = "Student not in class";
                return result;
            }
            if (@class.ChangeGroupDeadline < DateTime.Now)
            {
                result.Message = "Cannot change group at this time";
                return result;
            }
            var project = await _commonServices.GetProject(studentId);
            if (project != null)
            {
                result.Message = "Student currently in a group";
                return result;
            }
            var studentInNewGroupCount = await _unitOfWork.StudentRepository.Get().Where(x => x.ProjectId == joinGroupExist).CountAsync();
            if (@class.MaxMember <= studentInNewGroupCount)
            {
                result.Message = "Group is full";
                return result;
            }
            result.Message = string.Empty;
            result.Result = true;
            return result;
        }

        public async Task RequestToJoinGroup(JoinGroupRequest request, Guid studentId)
        {
            var memberHistory = new MemberHistory()
            {
                ReporterId = studentId,
                IPMSClassId = _commonServices.GetClass()!.Id,
                ProjectToId = request.GroupId,
                MemberSwapId = Guid.Empty,
                ProjectFromId = Guid.Empty
            };
            await _unitOfWork.MemberHistoryRepository.InsertAsync(memberHistory);
            await _unitOfWork.SaveChangesAsync();
            var studentForNoti = await _unitOfWork.ProjectRepository.Get().Where(x => x.Id == request.GroupId)
                                                                    .Include(x => x.Students).SelectMany(x => x.Students,
                                                                    (project, student) => new
                                                                    {
                                                                        student.InformationId
                                                                    }).ToListAsync();
            List<Task> sendMessageTasks = new List<Task>();
            foreach (var student in studentForNoti)
            {
                sendMessageTasks.Add(_messageService.SendMessage(new NotificationMessage
                {
                    AccountId = student.InformationId,
                    Message = "A student want to join your group",
                    Title = "Join Group Request"
                }));
            }
            await Task.WhenAll(sendMessageTasks);
        }

        public async Task<ValidationResultModel> CheckValidAssignLeaderRequest(AssignLeaderRequest request, Guid studentId)
        {
            var result = new ValidationResultModel
            {
                Message = "Cannot assign leader"
            };
            if (request.MemberId == studentId)
            {
                result.Message = "Can Only assign to another member";
                return result;
            }
            var project = await _commonServices.GetProject(studentId);
            if (project == null)
            {
                result.Message = "Not found Valid Project";
                return result;
            }
            await _unitOfWork.ProjectRepository.LoadExplicitProperty(project, nameof(Project.Students));
            var isMemberInGroup = project.Students.Any(x => x.InformationId == request.MemberId);
            if (!isMemberInGroup)
            {
                result.Message = "The student want to assign is not in your group";
                return result;
            }
            result.Message = string.Empty;
            result.Result = true;
            return result;
        }

        public async Task AssignLeader(AssignLeaderRequest request, Guid studentId)
        {
            //Remove role leader
            var oldLeader = await _userManager.FindByIdAsync(studentId.ToString());
            await _userManager.RemoveFromRoleAsync(oldLeader, UserRole.Leader.ToString());
            //Add role to new Leader
            var newLeader = await _userManager.FindByIdAsync(request.MemberId.ToString());
            await _userManager.AddToRoleAsync(newLeader, UserRole.Leader.ToString());
            //Send message to new leader
            var message = new NotificationMessage
            {
                AccountId = request.MemberId,
                Title = "New Leader Assignment",
                Message = "You have been assigned to become leader of your group"
            };
            await _messageService.SendMessage(message);
        }
        public async Task<ValidationResultModel> AddMemberValidators(Guid studentId, Guid projectId)
        {
            var result = new ValidationResultModel
            {
                Message = string.Empty,
                Result = true
            };
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id.Equals(studentId));
            if (user == null)
            {
                result.Message = "User does not existed";
                result.Result = false;
            }
            var project = await _unitOfWork.ProjectRepository.Get().FirstOrDefaultAsync(p => p.Id.Equals(projectId));
            if (project == null)
            {
                result.Message = "Project does not existed";
                result.Result = false;
                return result;
            }

            Guid currentSemesterId = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester!.Id;
            var studiesIn = (await _commonServices.GetStudiesIn(studentId)).Select(s => s.ClassId);
            var currentClass = await _commonServices.GetCurrentClass(studiesIn, currentSemesterId); // need not to check current Class because checked when get project
            if (currentClass.ChangeGroupDeadline < DateTime.Now)
            {
                result.Message = "Cannot add member at this time";
                return result;
            }
            await _unitOfWork.ProjectRepository.LoadExplicitProperty(project, nameof(Project.Students));
            if (currentClass.MaxMember <= project.Students.Count)
            {
                result.Message = "Group is full";
                return result;
            }

            return result;
        }
        public async Task AddMember(Guid studentId, Guid projectId) // need validation
        {
            /*var project = await _unitOfWork.ProjectRepository.Get().FirstOrDefaultAsync(p => p.Id.Equals(projectId));

            Guid currentSemesterId = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester!.Id;
            var studiesIn = (await _commonServices.GetStudiesIn(studentId)).Select(s => s.ClassId);
            var currentClass = await _commonServices.GetCurrentClass(studiesIn, currentSemesterId);
            var newMem = new Student
            {
                ProjectId = projectId,
                InformationId = studentId,
                ClassId = currentClass.Id
            };
            await _unitOfWork.StudentRepository.InsertAsync(newMem);
            await _unitOfWork.SaveChangesAsync();*/

            var student = await _unitOfWork.StudentRepository.Get().FirstOrDefaultAsync(s => s.InformationId.Equals(studentId));
            student.ProjectId = projectId;
            _unitOfWork.StudentRepository.Update(student);
            await _unitOfWork.SaveChangesAsync();

        }
    }
}
