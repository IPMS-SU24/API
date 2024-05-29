using IPMS.DataAccess.Models;
using System.Threading;

namespace IPMS.Business.Common.Singleton
{
    public sealed class CurrentSemesterInfo
    {
        public Semester? CurrentSemester { get; set; }
        private static object synclock = new object();
        private static CurrentSemesterInfo _instance;
        public bool IsCurrent
        { 
            get
            {
                var now = DateTime.Now;
                if(CurrentSemester == null) return false;
                return CurrentSemester.EndDate > now && CurrentSemester.StartDate <= now;
            } 
        }
        public static CurrentSemesterInfo Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (synclock)
                    {
                        if (_instance == null)
                        {
                            _instance = new CurrentSemesterInfo();
                        }
                    }
                }
                return _instance;
            }
        }
    }
}
