using IPMS.Business.Common.Models;

namespace IPMS.Business.Interfaces.Services
{
    public interface IBackgoundJobService
    {
        Task ProcessAddStudentToClass(StudentDataRow student, Guid classId, string serverDomain);
    }
}
