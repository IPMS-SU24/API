using IPMS.Business.Common.Enums;
using IPMS.Business.Common.Utils;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Responses.MemberHistory;
using IPMS.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IPMS.Business.Services
{
    public class MemberHistoryService : IMemberHistoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonServices _commonServices;
        // private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly UserManager<IPMSUser> _userManager;

        public MemberHistoryService(IUnitOfWork unitOfWork, ICommonServices commonServices,
                         //RoleManager<IdentityRole<Guid>> roleManager, 
                         UserManager<IPMSUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _commonServices = commonServices;
            //  _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<IQueryable<LoggedInUserHistoryResponse>> GetLoggedInUserHistories(Guid currentUserId)
        {
            

            // Find current project
            var project = await _commonServices.GetProject(currentUserId);

            // Find member of project
            var members = _unitOfWork.StudentRepository.Get().Where(s => s.ProjectId.Equals(project!.Id));

            // Find leader of project
            var allLeaders = (await _userManager.GetUsersInRoleAsync(UserRole.Leader.ToString())).Select(x => x.Id).ToList();
            var leaderId = (await members.FirstOrDefaultAsync(m => allLeaders.Contains(m.InformationId)))!.InformationId;
            IQueryable<MemberHistory> histories;
            if (leaderId.Equals(currentUserId)) // Note: Can ignore case get history from previous semester because get currentProject
            {
                histories = _unitOfWork.MemberHistoryRepository.Get().
                                            Where(mh => mh.ReporterId.Equals(currentUserId) || mh.MemberSwapId.Equals(mh.Id)
                                            || mh.ProjectFromId.Equals(project!.Id) || mh.ProjectToId.Equals(project.Id));
            }
            else
            {
                histories = _unitOfWork.MemberHistoryRepository.Get().
                                            Where(mh => mh.ReporterId.Equals(currentUserId) || mh.MemberSwapId.Equals(mh.Id));
            }
            // if have any rejected - needn't update anything -> Just show current status


            // Update if expired time
            // Find current class
            Guid currentSemesterId = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester!.Id;
            var studiesIn = (await _commonServices.GetStudiesIn(currentUserId)).Select(s => s.Id);
            var currentClass = await _commonServices.GetCurrentClass(studiesIn, currentSemesterId);


            // Create IQueryable response
            //  Type = ProjectFrom (currentProject) != null: Swap, else: Join
            //      Combine 3 status if 1 reject -> reject

            //  ProjectFrom = query project to find name -> found above


            //  ProjectTo = query project from projectToId


            //  MemberSwap = query IPMSUser from memberSwapId 

            return null;
        }
    }
}
