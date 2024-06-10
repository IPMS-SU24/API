using IPMS.Business.Interfaces.Services;
using IPMS.Business.Responses.MemberHistory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPMS.Business.Services
{
    public class MemberHistoryService : IMemberHistoryService
    {
        public async Task<IQueryable<LoggedInUserHistoryResponse>> GetLoggedInUserHistories(Guid currentUserId)
        {
            // Find current project
            // Find member of project 
            // Find leader of project

            // Create IQueryable response
            //  Type = ProjectFrom (currentProject) != null: Swap, else: Join
            //  ProjectFrom = query project to find name -> found above
            //  ProjectTo = query project from projectToId
            //  MemberSwap = query IPMSUser from memberSwapId 


            return null;
        }
    }
}
