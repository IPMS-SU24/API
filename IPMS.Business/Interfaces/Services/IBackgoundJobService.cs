using Hangfire;
using IPMS.Business.Common.Models;
using IPMS.Business.Models;

namespace IPMS.Business.Interfaces.Services
{
    public interface IBackgoundJobService
    {
        [AutomaticRetry(Attempts = 5)]
        [DisableConcurrentExecution(timeoutInSeconds: 10 * 60)]
        [Queue("import_student")]
        Task ProcessAddStudentToClass(StudentDataRow student, Guid classId);

        [AutomaticRetry(Attempts = 5)]
        [DisableConcurrentExecution(timeoutInSeconds: 10 * 60)]
        [Queue("import_student")]
        Task AddJobIdToStudent(string jobId, Guid classId, string email);

        [AutomaticRetry(Attempts = 5)]
        [DisableConcurrentExecution(timeoutInSeconds: 10 * 60)]
        [Queue("import_class")]
        Task ProcessAddClassToSemester(ClassDataRow @class, Guid semesterId);
    }
}
