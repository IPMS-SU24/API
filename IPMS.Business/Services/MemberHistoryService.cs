using IPMS.Business.Common.Enums;
using IPMS.Business.Common.Exceptions;
using IPMS.Business.Common.Utils;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Responses.MemberHistory;
using IPMS.DataAccess.Common.Enums;
using IPMS.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace IPMS.Business.Services
{
    public class MemberHistoryService : IMemberHistoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonServices _commonServices;
        private readonly UserManager<IPMSUser> _userManager;

        public MemberHistoryService(IUnitOfWork unitOfWork, ICommonServices commonServices, UserManager<IPMSUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _commonServices = commonServices;
            _userManager = userManager;
        }

        public async Task<IQueryable<LoggedInUserHistoryResponse>> GetLoggedInUserHistories(Guid currentUserId) 
        {
            // Will not kick user

            // Find current class
            Guid currentSemesterId = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester!.Id;
            var studiesIn = (await _commonServices.GetStudiesIn(currentUserId)).Select(s => s.Id);
            var currentClass = await _commonServices.GetCurrentClass(studiesIn, currentSemesterId);

            if (currentClass == null)
                throw new DataNotFoundException("Current user isn't in any class");

            Guid? leaderId = null; // default current user is freedom

            // Find current project
            var project = await _commonServices.GetProject(currentUserId);

            if (project != null) // user currently in project
            {
                var members = _unitOfWork.StudentRepository.Get().Where(s => s.ProjectId.Equals(project!.Id)); // Find member of project

                var allLeaders = (await _userManager.GetUsersInRoleAsync(UserRole.Leader.ToString())).Select(x => x.Id).ToList(); // Find leader of project

                leaderId = (await members.FirstOrDefaultAsync(m => allLeaders.Contains(m.InformationId)))!.InformationId; // find leader of current user's project

            }

            // Find histories
            IQueryable<MemberHistory> histories;
            //  Note: find with current class to ignore case re-study
            if (leaderId != null && leaderId.Equals(currentUserId))  // current user also a leader
            {
                histories = _unitOfWork.MemberHistoryRepository.Get().
                                            Where(mh => (mh.ReporterId.Equals(currentUserId) || mh.MemberSwapId.Equals(mh.Id)
                                            || mh.ProjectFromId.Equals(project!.Id) || mh.ProjectToId.Equals(project.Id))
                                                && mh.IPMSClassId.Equals(currentClass!.Id));
            }
            else // not a leader
            {
                histories = _unitOfWork.MemberHistoryRepository.Get().
                                            Where(mh => (mh.ReporterId.Equals(currentUserId) || mh.MemberSwapId.Equals(mh.Id))
                                                    && mh.IPMSClassId.Equals(currentClass!.Id));
            }

            // if have any rejected - needn't update anything -> Just show current status

            // Update if expired
            if (currentClass!.ChangeGroupDeadline <= DateTime.Now)
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
            //  Type = ProjectFrom (currentProject) != null: Swap, else: Join
            //      Combine 3 status if 1 reject -> reject

            //  ProjectFrom = query project to find name -> found above


            //  ProjectTo = query project from projectToId


            //  MemberSwap = query IPMSUser from memberSwapId 
            var users = _userManager.Users.ToList();
            var projects = _unitOfWork.ProjectRepository.Get().ToList();

            IQueryable<LoggedInUserHistoryResponse> response = histories.Select(h => new LoggedInUserHistoryResponse
            {
                Id = h.Id,
                LeaderId = new Guid(),
                RequestType = (h.ProjectFromId == null) ? "join" : "swap",
                Requester =  GetUser(users, h.ReporterId), // cannot use async await in here, cannot query
                MemberSwap = GetUser(users, h.MemberSwapId),
                ProjectFrom  = GetProject(projects, h.ProjectFromId), // Iqueryable will be borrow if query db again -> so that query before and just linq select
                ProjectTo = GetProject(projects, h.ProjectToId),
                Status = GetFinalStatus(h),
                CreateAt = h.CreatedDate,
            });

            return response;
        }
        private RequestStatus GetFinalStatus(MemberHistory history)
        {
            if (history.MemberSwapId != null && history.MemberSwapStatus == RequestStatus.Waiting)
            {
                return RequestStatus.Rejected;

            }

            else if (history.ProjectFromId != null && history.ProjectFromStatus == RequestStatus.Waiting)
            {
                return RequestStatus.Rejected;

            }

            else if (history.ProjectToId != null && history.ProjectToStatus == RequestStatus.Waiting)
            {
                return RequestStatus.Rejected;

            }
            return RequestStatus.Approved;

        }
        private GeneralObjectInformation GetUser(List<IPMSUser> users, Guid? userId)
        {
            if (userId == null) // case join
                return null;

            var user = users.FirstOrDefault(u => u.Id.Equals(userId));

            if (user == null) // validate user
                return null;

            return new GeneralObjectInformation
            {
                Id = user.Id,
                Name = user.FullName
            };
        }

        private GeneralObjectInformation GetProject(List<Project> projects, Guid? projectId)
        {
            if (projectId == null) // case join
                return null;
            var project = projects.FirstOrDefault(p => p.Id.Equals(projectId));

            if (project == null) // validate project
                return null;

            return new GeneralObjectInformation
            {
                Id = project.Id,
                Name = project.GroupName
            };

        }
        private IQueryable<MemberHistory> UpdateExpiredRequest(IQueryable<MemberHistory> histories)
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

        // Can implement but not improve readable
     /*   private bool ReviewStatus(Guid? ReviewId, RequestStatus currentStatus, RequestStatus expectStatus)
        {
            if (ReviewStatus != null && currentStatus == expectStatus)
            {
                return true;
            }
            return false;
        }*/
    }
}
