using IPMS.Business.Common.Enums;
using IPMS.DataAccess.Models;
using Microsoft.AspNetCore.Http;

namespace IPMS.Business.Interfaces.Services
{
    public interface ICommonServices
    {
        Task<List<Student>> GetStudiesIn(Guid currentUserId);
        Task<IPMSClass?> GetCurrentClass(IEnumerable<Guid> studiesIn, Guid currentSemesterId);
        Task<IPMSClass?> GetCurrentClass(IEnumerable<Guid> studiesIn);
        Task<IPMSClass?> GetCurrentClass(Guid studentId);
        Task<Project?> GetProject(Guid currentUserId);
        Task<Topic?> GetProjectTopic(Guid projectId);
        Task<IEnumerable<ProjectSubmission>> GetProjectSubmissions(Guid projectId);
        //Prerequisite: List all submission belong to the project of the assessment
        Task<AssessmentStatus> GetAssessmentStatus(Guid assessmentId, IEnumerable<ProjectSubmission> submissionList);
        Task<AssessmentStatus> GetBorrowIoTStatus(Guid projectId, IPMSClass @class);
        Task<int> GetRemainComponentQuantityOfLecturer(Guid lecturerId, Guid componentId);
        Task<List<Guid>> GetAllCurrentProjectsOfLecturer(Guid lecturerId);
        Task<List<IPMSClass>> GetAllCurrentClassesOfLecturer(Guid lecturerId);
        (DateTime startDate, DateTime endDate) GetAssessmentTime(Guid assessmentId, Guid classId);
        AssessmentStatus GetChangeTopicStatus(Topic? topic, DateTime changeTopicDeadline, DateTime changeGroupDeadline);
        Task SetCommonSessionUserEntity(Guid currentUserId);
        IPMSClass? GetClass();
        Project? GetProject();

    }
}
