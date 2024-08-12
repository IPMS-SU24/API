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
        [JsonIgnore]
        Guid Id { get; set; }
        decimal Percentage { get; set; }
        string Name { get; set; }
    }
    public class AssessmentGrade : IGradeInfo
    {
        public Guid Id { get; set; }
        public decimal Percentage { get; set; }
        public string Name { get; set; } = null!;
        public List<SubmissionGrade> SubmissionGrades { get; set; } = new();
        [JsonIgnore]
        public decimal? AssessmentAvg 
        { 
            get
            {
                if (SubmissionGrades.Any(x => !x.Grade.HasValue)) return null;
                return SubmissionGrades.Select(ps => ps.Grade * (ps.Percentage / 100)).Sum();
            }
        }
        [JsonIgnore]
        public int Order { get; set; }
    }
    public class SubmissionGrade : IGradeInfo
    {
        public Guid Id { get; set; }
        public decimal Percentage { get; set; }
        public string Name { get; set; } = null!;
        public decimal? Grade { get; set; }
        public List<CommitteeResponse> Response { get; set; } = new();
    }

    public class CommitteeResponse
    {
        public string Name { get; set; }
        public string? Response { get; set; }
    }
}
