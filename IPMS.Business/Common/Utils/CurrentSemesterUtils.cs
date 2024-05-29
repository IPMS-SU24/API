using IPMS.Business.Common.Exceptions;
using IPMS.Business.Common.Singleton;
using IPMS.Business.Interfaces;
using IPMS.DataAccess.Common.Extensions;
using Microsoft.EntityFrameworkCore;

namespace IPMS.Business.Common.Utils
{
    public static class CurrentSemesterUtils
    {
        public static async Task<CurrentSemesterInfo> GetCurrentSemester(IUnitOfWork unitOfWork)
        {
            if(CurrentSemesterInfo.Instance.IsCurrent) return CurrentSemesterInfo.Instance;
            var now = DateTime.Now;
            CurrentSemesterInfo.Instance.CurrentSemester = await unitOfWork.SemesterRepository
                                                            .Get().Where(x=>x.EndDate > now && x.StartDate < now).GetQueryActive().Include(x=>x.Syllabus).ThenInclude(x=>x.Assessments).ThenInclude(x=>x.Modules).FirstOrDefaultAsync();
            if (CurrentSemesterInfo.Instance.CurrentSemester == null) throw new NoCurrentSemesterException();
            return CurrentSemesterInfo.Instance;
        }
    }
}
