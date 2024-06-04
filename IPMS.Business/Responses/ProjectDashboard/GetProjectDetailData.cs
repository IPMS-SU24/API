using IPMS.Business.Common.Enums;
using Microsoft.OpenApi.Extensions;
using System.ComponentModel.DataAnnotations;

namespace IPMS.Business.Responses.ProjectDashboard
{
    public class GetProjectDetailData
    {
        public Guid ProjectId { get; set; }
        public string TopicName { get; set; } = null!;
        public List<AssessmentDetail> Assessments { get; set; } = new List<AssessmentDetail>();
        public SubmissionCount Submission { get; set; } = new();
    }
    public class SubmissionCount
    {
        public int Done { get; set; }
        public int Total { get; set; }
    }
    public class AssessmentDetail
    {
        public Guid AssessmentId { get; set; }
        public string AssessmentName { get; set; } = null!;
        //[JsonIgnore]
        public AssessmentStatus AssessmentStatus { get; set; }
        public string Status => AssessmentStatus.GetAttributeOfType<DisplayAttribute>().Name ?? string.Empty;
    }
}
