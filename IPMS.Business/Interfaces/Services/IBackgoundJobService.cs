using Hangfire;
using IPMS.Business.Common.Models;

namespace IPMS.Business.Interfaces.Services
{
    public interface IBackgoundJobService
    {
        [AutomaticRetry(Attempts = 5)]
        [DisableConcurrentExecution(timeoutInSeconds: 10 * 60)]
        Task ProcessAddStudentToClass(StudentDataRow student, Guid classId, string serverDomain);
        Task AddJobIdToStudent(string jobId, Guid classId, string email);
    }
}
