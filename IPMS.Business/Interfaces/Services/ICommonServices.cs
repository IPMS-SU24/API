using IPMS.Business.Common.Enums;
using IPMS.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPMS.Business.Interfaces.Services
{
    public interface ICommonServices
    {
        Task<List<Student>> GetStudiesIn(Guid currentUserId);
        Task<IPMSClass?> GetCurrentClass(IEnumerable<Guid> studiesIn, Guid currentSemesterId);
        Task<(DateTime StartDate, DateTime EndDate)> GetAssessmentTime(Guid assessmentId, IUnitOfWork unitOfWork);
        Task<Project?> GetProject(Guid currentUserId);
        Task<Topic?> GetProjectTopic(Guid projectId);
        Task<IEnumerable<ProjectSubmission>> GetProjectSubmissions(Guid projectId);
        //Prerequisite: List all submission belong to the project of the assessment
        Task<AssessmentStatus> GetAssessmentStatus(Guid assessmentId, IEnumerable<ProjectSubmission> submissionList);
        Task<AssessmentStatus> GetBorrowIoTStatus(Guid projectId, IPMSClass @class);
        Task<int> GetRemainComponentQuantityOfLecturer(Guid lecturerId, Guid componentId);
        Task<List<Guid>> GetAllCurrentProjectsOfLecturer(Guid lecturerId);
    }
}
