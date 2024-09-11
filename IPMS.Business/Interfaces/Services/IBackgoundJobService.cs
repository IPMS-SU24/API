using Hangfire;
using IPMS.Business.Common.Models;
using IPMS.Business.Models;

namespace IPMS.Business.Interfaces.Services
{
    public interface IBackgoundJobService
    {
        [AutomaticRetry(Attempts = 5)]
        [DisableConcurrentExecution(timeoutInSeconds: 10 * 60)]
        //[Queue("import_student")]
        [Queue("import_class")]
        Task ProcessAddStudentToClass(StudentDataRow student, Guid classId, string classCode);

        [AutomaticRetry(Attempts = 5)]
        [DisableConcurrentExecution(timeoutInSeconds: 10 * 60)]
        //[Queue("import_student")]
        [Queue("import_class")]
        Task AddJobIdToStudent(string jobId, Guid classId, string email);

        [AutomaticRetry(Attempts = 5)]
        [DisableConcurrentExecution(timeoutInSeconds: 10 * 60)]
        [Queue("import_class")]
        Task ProcessAddClassToSemester(ClassDataRow @class, Guid semesterId);

        [AutomaticRetry(Attempts = 5)]
        [DisableConcurrentExecution(timeoutInSeconds: 10 * 60)]
        [Queue("import_class")]
        Task ProcessAddAllClassInfoToSemester(Guid semesterId, string fileName);

        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(timeoutInSeconds: 10 * 60)]
        [Queue("import_class")]
        Task ProcessAddAllStudentListToClass(string fileName, string classCode, Guid semesterId, int headerRowNumber, string[] headerTitles);
    }
}
