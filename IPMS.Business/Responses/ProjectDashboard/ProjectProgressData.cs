using IPMS.Business.Common.Enums;
using Microsoft.OpenApi.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IPMS.Business.Responses.ProjectDashboard
{
    public class ProjectProgressData
    {
        public string ProjectName { get; set; }
        public Guid ProjectId { get; set; }
        public TopicInformation TopicInfo { get; set; }
        public BorrowInformation BorrowInfo { get; set; }
        public List<AssessmentInformation> Assessments { get; set; } = new List<AssessmentInformation>();
    }
    public class TopicInformation
    {
        //[JsonIgnore]
        public AssessmentStatus AssessmentStatus { get; set; }
        public string Status
        {
            get => AssessmentStatus.GetAttributeOfType<DisplayAttribute>().Name ?? string.Empty;
        }
        public string TopicName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime EndDate { get; set; }
    }
    public class BorrowInformation
    {
        //[JsonIgnore]
        public AssessmentStatus AssessmentStatus { get; set; }
        public string Status
        {
            get => AssessmentStatus.GetAttributeOfType<DisplayAttribute>().Name ?? string.Empty;
        }
        public DateTime EndDate { get; set; }
        public List<BorrowIoTComponentInformation> IoTComponents { get; set; } = new();
    }
    public class BorrowIoTComponentInformation
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
    }
    public class AssessmentInformation
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
        public decimal Percentage { get; set; }
        //[JsonIgnore]
        public AssessmentStatus AssessmentStatus { get; set; }
        
        public string Status
        {
            get => AssessmentStatus.GetAttributeOfType<DisplayAttribute>().Name ?? string.Empty;
        }
        public DateTime EndDate { get; set; }
    }
}
