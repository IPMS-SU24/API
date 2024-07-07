using Hangfire.Server;

namespace IPMS.Business.Common.Hangfire
{
    public class JobContext : IServerFilter
    {
        private static string _jobId;

        public static string CurrentJobId { get { return _jobId; } set { _jobId = value; } }

        public void OnPerformed(PerformedContext context)
        {
            
        }

        public void OnPerforming(PerformingContext context)
        {
            CurrentJobId = context.BackgroundJob.Id;
        }
    }
}
