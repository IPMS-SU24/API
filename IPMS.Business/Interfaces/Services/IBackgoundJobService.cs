using IPMS.Business.Common.Models;

namespace IPMS.Business.Interfaces.Services
{
    public interface IBackgoundJobService
    {
        Task ProcessAddStudentToClass(StudentDataRow student, Guid classId, string serverDomain);
        Task AddJobIdToStudent(string jobId, Guid classId, string email);
    }
}
