﻿using Amazon.SQS.Model;
using IPMS.Business.Common.Enums;
using IPMS.Business.Common.Exceptions;
using IPMS.Business.Common.Extensions;
using IPMS.Business.Common.Utils;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Models;
using IPMS.Business.Requests.MemberHistory;
using IPMS.Business.Responses.MemberHistory;
using IPMS.DataAccess.Common.Enums;
using IPMS.DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Utilities.Collections;
using System.Runtime.CompilerServices;

namespace IPMS.Business.Services
{
    public class MemberHistoryService : IMemberHistoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonServices _commonServices;
        private readonly UserManager<IPMSUser> _userManager;
        private readonly IStudentGroupService _studentGroupService;
        private readonly IHttpContextAccessor _context;
        private MemberHistory history { get; set; }
        private readonly IMessageService _messageService;

        public MemberHistoryService(IUnitOfWork unitOfWork, ICommonServices commonServices, UserManager<IPMSUser> userManager, IStudentGroupService studentGroupService, IHttpContextAccessor context, IMessageService messageService)
        {
            _unitOfWork = unitOfWork;
            _commonServices = commonServices;
            _userManager = userManager;
            _studentGroupService = studentGroupService;
            _context = context;
            _messageService = messageService;
        }
        private async Task<Guid?> GetLeaderId(Guid projectId)
        {
            Guid? leaderId = null;
            var members = _unitOfWork.StudentRepository.Get().Where(s => s.ProjectId.Equals(projectId)); // Find member of project

            var allLeaders = (await _userManager.GetUsersInRoleAsync(UserRole.Leader.ToString())).Select(x => x.Id).ToList(); // Find leader of project

            leaderId = (await members.FirstOrDefaultAsync(m => allLeaders.Contains(m.InformationId))).InformationId; // find leader of current user's project
            return leaderId;
        }

        public async Task<List<LoggedInUserHistoryResponse>> GetLoggedInUserHistories(Guid currentUserId)
        {
            // Will not kick user

            // Find current class
            IPMSClass? @class = _context.HttpContext.Session.GetObject<IPMSClass?>("Class");
            if (@class == null)
                throw new DataNotFoundException("Current user isn't in any class");

            Guid? leaderId = null; // default current user is freedom

            // Find current project
            Project? project = _context.HttpContext.Session.GetObject<Project?>("Project");

            if (project != null) // user currently in project
            {
                leaderId = await GetLeaderId(project.Id);

            }

            // Find histories
            //IQueryable<MemberHistory> histories;
            List<MemberHistory> histories;
            //  Note: find with current class to ignore case re-study
            if (leaderId != null && leaderId.Equals(currentUserId))  // current user also a leader
            {
                histories = await _unitOfWork.MemberHistoryRepository.Get().
                                            Where(mh => (mh.ReporterId.Equals(currentUserId) || mh.MemberSwapId.Equals(currentUserId)
                                            || mh.ProjectFromId.Equals(project!.Id) || mh.ProjectToId.Equals(project.Id))
                                                && mh.IPMSClassId.Equals(@class!.Id)).ToListAsync();
            }
            else // not a leader
            {
                histories = await _unitOfWork.MemberHistoryRepository.Get().
                                            Where(mh => (mh.ReporterId.Equals(currentUserId) || mh.MemberSwapId.Equals(currentUserId))
                                                    && mh.IPMSClassId.Equals(@class!.Id)).ToListAsync();
            }

            // if have any rejected - needn't update anything -> Just show current status

            // Update if expired
            if (@class!.ChangeGroupDeadline <= DateTime.Now)
            {
                int expiredReviews = histories.Count(h => (h.MemberSwapId != null && h.MemberSwapStatus == RequestStatus.Waiting)
                                                            || (h.ProjectFromId != null && h.ProjectFromStatus == RequestStatus.Waiting)
                                                            || (h.ProjectToId != null && h.ProjectToStatus == RequestStatus.Waiting));
                if (expiredReviews > 0) // set status
                {
                    histories = UpdateExpiredRequest(histories);
                }
            }

            // Create IQueryable response
            //  Type = ProjectFrom (currentProject) != Guid.Empty: Swap, else: Join
            //      Combine 3 status if 1 reject -> reject

            //  ProjectFrom = query project to find name -> found above


            //  ProjectTo = query project from projectToId


            //  MemberSwap = query IPMSUser from memberSwapId 

            // leaderId : 
            //  case swap: just need leaderId of current user -> memberswap, leader project to can review, leader project from can review
            //  case join: current user -> leaderId: dont have --> Guid.Empty. Leader project to: can review
            var users = _userManager.Users.ToList();
            var projects = _unitOfWork.ProjectRepository.Get().ToList();
            var response = histories.Select(h => new LoggedInUserHistoryResponse
            {
                Id = h.Id,
                LeaderId = leaderId == null ? null : leaderId.Value,
                RequestType = (h.ProjectFromId == null) ? "join" : "swap",
                Requester = GetUser(users, h.ReporterId, null), // cannot use async await in here, cannot query
                MemberSwap = GetUser(users, h.MemberSwapId, h.MemberSwapStatus),
                ProjectFrom = GetProject(projects, h.ProjectFromId, h.ProjectFromStatus), // Iqueryable will be borrow if query db again -> so that query before and just linq select
                ProjectTo = GetProject(projects, h.ProjectToId, h.ProjectToStatus),
                Status = h.FinalStatus,
                CreateAt = h.CreatedAt,
            }).ToList();

            return response;

        }
        private GeneralObjectInformation GetUser(List<IPMSUser> users, Guid? userId, RequestStatus? status)
        {
            if (userId == null) // case join
                return null;

            var user = users.FirstOrDefault(u => u.Id.Equals(userId));

            if (user == null) // validate user
                return null;

            GeneralObjectInformation information = new GeneralObjectInformation
            {
                Id = user.Id,
                Name = user.FullName
            };
            if (status != null)
            {
                information.Status = status;
            }

            return information;


        }

        private GeneralObjectInformation GetProject(List<Project> projects, Guid? projectId, RequestStatus? status)
        {
            if (projectId == null) // case join
                return null;
            var project = projects.FirstOrDefault(p => p.Id.Equals(projectId));

            if (project == null) // validate project
                return null;

            GeneralObjectInformation information = new GeneralObjectInformation
            {
                Id = project.Id,
                Name = $"Group {project.GroupNum}"
            };

            if (status != null)
            {
                information.Status = status;
            }

            return information;

        }
        private List<MemberHistory> UpdateExpiredRequest(List<MemberHistory> histories)
        {
            foreach (var history in histories)
            {
                if (history.MemberSwapId != null && history.MemberSwapStatus == RequestStatus.Waiting)
                {
                    history.MemberSwapStatus = RequestStatus.Rejected;
                }

                if (history.ProjectFromId != null && history.ProjectFromStatus == RequestStatus.Waiting)
                {
                    history.ProjectFromStatus = RequestStatus.Rejected;

                }

                if (history.ProjectToId != null && history.ProjectToStatus == RequestStatus.Waiting)
                {
                    history.ProjectToStatus = RequestStatus.Rejected;

                }
            }
            return histories;
        }
        private async Task SendMailReject(MemberHistory history, string type)
        {
            var project = await _unitOfWork.ProjectRepository.Get().Where(p => p.Id.Equals(history.ProjectToId)).FirstOrDefaultAsync();
            string typeRequest = type == "join" ? "Join" : "Swap";
            var notificationMessageToLecturer = new NotificationMessage
            {
                AccountId = history.ReporterId,
                Title = $"{typeRequest} Request",
                Message = $"Your {type} request to Group {project.GroupNum} has been rejected"
            };
            await _messageService.SendMessage(notificationMessageToLecturer);
        }
        private async Task SetStatusReject(MemberHistory history, string type)
        {
            if (type == "join")
            {
                history.ProjectToStatus = RequestStatus.Rejected;
            }
            else if (type == "swap")
            {
                history.ProjectFromStatus = RequestStatus.Rejected;
                history.ProjectToStatus = RequestStatus.Rejected;
                history.MemberSwapStatus = RequestStatus.Rejected;
            }
            await _unitOfWork.SaveChangesAsync();
            _unitOfWork.MemberHistoryRepository.Update(history);
            await SendMailReject(history, type);

        }

        public async Task<ValidationResultModel> UpdateRequestStatusValidators(UpdateRequestStatusRequest request, Guid studentId)
        {

            var result = new ValidationResultModel
            {
                Message = "Operation did not successfully"
            };

            history = await _unitOfWork.MemberHistoryRepository.Get().FirstOrDefaultAsync(mh => mh.Id.Equals(request.Id));
            if (history == null)
            {
                result.Message = "History does not exist";
                return result;
            }
            IPMSUser reqUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Id.Equals(history.ReporterId));
            if (reqUser == null)
            {
                result.Message = "Requester is not exist";
                return result;
            }
            Project reqUserProject = await _commonServices.GetProject(reqUser.Id);

            IPMSClass? @class = _context.HttpContext.Session.GetObject<IPMSClass?>("Class");
            // need not to check current Class because checked when get project
            if (@class.ChangeGroupDeadline < DateTime.Now) // Need to set status reject
            {
                result.Message = "Cannot review at this time";
                return result;
            }
            Project? project = _context.HttpContext.Session.GetObject<Project?>("Project");

            if (request.Type == "join")
            {
                if (history.ProjectFromId != null  // validation data
                    || history.MemberSwapId != null)
                {
                    result.Message = "Request is not correct";
                    return result;
                }

                if (reqUserProject != null)
                {
                    await SetStatusReject(history, request.Type);  // Need to set status reject
                    result.Message = "Student currently in project";
                    return result;
                }

                Project? joinPrj = _context.HttpContext.Session.GetObject<Project?>("Project");
                await _unitOfWork.ProjectRepository.LoadExplicitProperty(joinPrj, nameof(Project.Students));

                if (@class.MaxMember == joinPrj.Students.Count)
                {
                    await SetStatusReject(history, request.Type);  // Need to set status reject
                    result.Message = "Group is full";
                    return result;
                }


            }
            else if (request.Type == "swap")
            {
                if (history.ProjectFromId == null || history.ProjectFromId == null // validation data
                    || history.ProjectToId == null || history.ProjectToId == null
                    || history.MemberSwapId == null || history.MemberSwapId == null)
                {
                    result.Message = "Request is not correct";
                    return result;
                }
                // requester case
                if (history.ReporterId == history.MemberSwapId)
                {
                    await SetStatusReject(history, request.Type);  // Need to set status reject
                    result.Message = "Can not swap itself";
                    return result;
                }

                if (reqUserProject == null)
                {
                    await SetStatusReject(history, request.Type);  // Need to set status reject
                    result.Message = "Requester is not in project";
                    return result;
                }
                if (reqUserProject.Id != history.ProjectFromId)
                {
                    await SetStatusReject(history, request.Type);  // Need to set status reject
                    result.Message = "Requester is not in project";
                    return result;
                }
                var leaderReqUserProjectId = await GetLeaderId(reqUserProject.Id);
                if (leaderReqUserProjectId == reqUser.Id)
                {
                    await SetStatusReject(history, request.Type);  // Need to set status reject
                    result.Message = "Requester currently is a leader";
                    return result;
                }

                // Case for project from || project to || member swap
                if (project == null) // current reviewing is not in project -> No access
                {
                    await SetStatusReject(history, request.Type);  // Need to set status reject
                    result.Message = "Reviewer is not in project";
                    return result;
                }

                Guid? leaderId = await GetLeaderId(project.Id); // get leader of project

                // check case Id for project or member swap
                if ((history.ProjectFromId == request.ReviewId && request.ReviewId == project.Id) // review (project to || project from --> need current user is in role leader) &&  (user in project to || project from)
                       || (history.ProjectToId == request.ReviewId && request.ReviewId == project.Id))
                {
                    if (leaderId != studentId)
                    {
                        result.Message = "Current user is not leader";
                        return result;
                    }
                }
                else if (history.MemberSwapId != request.ReviewId) // not case leader -> case member swap
                {
                    result.Message = "No information about reviewing request";
                    return result;
                }

                if (request.ReviewId == studentId) // case member swap - can ignore case project from || project to because GUID is global unique!
                {
                    if (project.Id != history.ProjectToId)   // case member changed to another group // Need to set status reject
                    {
                        await SetStatusReject(history, request.Type);
                        result.Message = "Member swap is not in project";
                        return result;
                    }

                    if (project.Id == history.ProjectFromId)
                    {
                        await SetStatusReject(history, request.Type);  // Need to set status reject
                        result.Message = "Cannot swap member in same project";
                        return result;
                    }

                    // ** if not leader of project from || project to --> Out at line 283 --> Scope is leader project from, project to
                    if (leaderId == request.ReviewId) // case memberSwap = leader
                    {
                        await SetStatusReject(history, request.Type);  // Need to set status reject
                        result.Message = "Member swap currently is a leader";
                        return result;
                    }
                }

            }
            else // difference case join/swap
            {
                result.Message = request.Type + " does not exist";
                return result;
            }

            result.Message = string.Empty;
            result.Result = true;
            return result;
        }
        public async Task UpdateRequestStatus(UpdateRequestStatusRequest request)
        {
            if (request.Type == "join")
            {
                history.ProjectToStatus = request.Status;

            }
            else if (request.Type == "swap")
            {
                if (history.ProjectFromId == request.ReviewId)
                {
                    history.ProjectFromStatus = request.Status;
                }
                else if (history.ProjectToId == request.ReviewId)
                {
                    history.ProjectToStatus = request.Status;
                }
                else if (history.MemberSwapId == request.ReviewId)
                {
                    history.MemberSwapStatus = request.Status;
                }
            }

            // save changes
            _unitOfWork.MemberHistoryRepository.Update(history);
            await _unitOfWork.SaveChangesAsync();

            if (request.Status == RequestStatus.Approved)
            {
                await ChangeGroupMember(request);
            }
            else
            {
                await SendMailReject(history, request.Type);
            }
        }

        private async Task ChangeGroupMember(UpdateRequestStatusRequest request)
        {
            if (request.Type == "join")
            {
                await _studentGroupService.AddMember(history.ReporterId, (Guid)history.ProjectToId!);

                var project = await _unitOfWork.ProjectRepository.Get().Where(p => p.Id.Equals(history.ProjectToId)).FirstOrDefaultAsync();
                var notificationMessageToLecturer = new NotificationMessage
                {
                    AccountId = history.ReporterId,
                    Title = $"Join Request",
                    Message = $"Your join request to Group {project.GroupNum} has been accepted"
                };
                await _messageService.SendMessage(notificationMessageToLecturer);

            }
            else if (request.Type == "swap")
            {
                if (history.ProjectFromStatus == RequestStatus.Approved && history.ProjectToStatus == RequestStatus.Approved && history.MemberSwapStatus == RequestStatus.Approved)
                {
                    //   await _studentGroupService.RemoveMember(history.ReporterId);
                    //   await _studentGroupService.RemoveMember((Guid)history.MemberSwapId!);

                    await _studentGroupService.AddMember(history.ReporterId, (Guid)history.ProjectToId!);
                    await _studentGroupService.AddMember((Guid)history.MemberSwapId!, (Guid)history.ProjectFromId!);

                    var project = await _unitOfWork.ProjectRepository.Get().Where(p => p.Id.Equals(history.ProjectToId)).FirstOrDefaultAsync();
                    var notificationMessageToLecturer = new NotificationMessage
                    {
                        AccountId = history.ReporterId,
                        Title = $"Swap Request",
                        Message = $"Your swap request to Group {project.GroupNum} has been accepted"
                    };
                    await _messageService.SendMessage(notificationMessageToLecturer);
                }
            }
        }
    }
}
