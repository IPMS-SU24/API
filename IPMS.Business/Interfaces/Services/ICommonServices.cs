using IPMS.Business.Common.Enums;
using IPMS.DataAccess.Models;
using Microsoft.AspNetCore.Http;

namespace IPMS.Business.Interfaces.Services
{
    public interface ICommonServices
    {
        Task<List<Student>> GetStudiesIn(Guid currentUserId);
        [Obsolete("Using GetClass From Session Instead")]
        Task<IPMSClass?> GetCurrentClass(IEnumerable<Guid> studiesIn, Guid currentSemesterId);
        [Obsolete("Using GetClass From Session Instead")]
        Task<IPMSClass?> GetCurrentClass(IEnumerable<Guid> studiesIn);
        [Obsolete("Using GetClass From Session Instead")]
        Task<IPMSClass?> GetCurrentClass(Guid studentId);
        Task<(DateTime StartDate, DateTime EndDate)> GetAssessmentTime(Guid assessmentId, IUnitOfWork unitOfWork);
        [Obsolete("Using GetProject From Session Instead")]
        Task<Project?> GetProject(Guid currentUserId);
        Task<Topic?> GetProjectTopic(Guid projectId);
        Task<IEnumerable<ProjectSubmission>> GetProjectSubmissions(Guid projectId);
        //Prerequisite: List all submission belong to the project of the assessment
        Task<AssessmentStatus> GetAssessmentStatus(Guid assessmentId, IEnumerable<ProjectSubmission> submissionList);
        Task<AssessmentStatus> GetBorrowIoTStatus(Guid projectId, IPMSClass @class);
        Task<int> GetRemainComponentQuantityOfLecturer(Guid lecturerId, Guid componentId);
        Task<List<Guid>> GetAllCurrentProjectsOfLecturer(Guid lecturerId);
        Task<(DateTime startDate, DateTime endDate)> GetAssessmentTime(Guid lecturerId);
        AssessmentStatus GetChangeTopicStatus(Topic? topic, DateTime changeTopicDeadline, DateTime changeGroupDeadline);
        Task SetCommonSessionUserEntity(Guid currentUserId);
        IPMSClass? GetClass();
        Project? GetProject();

    }
}
