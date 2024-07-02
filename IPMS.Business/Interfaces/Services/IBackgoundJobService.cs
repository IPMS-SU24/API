using IPMS.Business.Common.Models;

namespace IPMS.Business.Interfaces.Services
{
    public interface IBackgoundJobService
    {
        Task ProcessAddStudentToClass(List<StudentDataRow> students, Guid classId, string serverDomain);
    }
}
