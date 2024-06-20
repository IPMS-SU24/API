using IPMS.Business.Responses.ProjectSubmission;

namespace IPMS.Business.Responses.ProjectPreference
{
    public class ProjectPreferenceResponse
    {
        public string TopicTitle { get; set; }
        public Guid? LecturerId { get; set; }
        public string LecturerName { get; set; }
        public string Semester { get; set; }
        public string SemesterCode { get; set; }
        public string Description { get; set; }
        public List<ProjectSubmissionResponse> ProjectSubmissions { get; set; } = new();
    }
}
