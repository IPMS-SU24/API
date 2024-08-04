using System.Text.Json.Serialization;

namespace IPMS.Business.Responses.ProjectSubmission
{
    public class GetGradeResponse
    {
        public List<AssessmentGrade> AssessmentGrades { get; set; } = new();
        public decimal? Total { get; set; }
    }
    public interface IGradeInfo
    {
        decimal Percentage { get; set; }
        string Name { get; set; }
    }
    public class AssessmentGrade : IGradeInfo
    {
        public decimal Percentage { get; set; }
        public string Name { get; set; } = null!;
        public List<SubmissionGrade> SubmissionGrades { get; set; } = new();
        [JsonIgnore]
        public decimal? AssessmentAvg { get; set; }
    }
    public class SubmissionGrade : IGradeInfo
    {
        public decimal Percentage { get; set; }
        public string Name { get; set; } = null!;
        public decimal? Grade { get; set; }
    }
}
