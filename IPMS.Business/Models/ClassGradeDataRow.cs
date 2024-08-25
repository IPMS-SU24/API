namespace IPMS.Business.Models
{
    public class ClassGradeDataRow
    {
        public Guid StudentTechnicalId { get; set; }
        public Guid? ProjectTechnicalId { get; set; }
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public int? Group { get; set; }
        public List<ExportAssessmentGrade> AssessmentGrades { get; set; } = new();
        public decimal? Total { get; set; }
        public decimal? Contribute { get; set; }
        public decimal? Final { get; set; }
    }
    public class ExportAssessmentGrade
    {
        public decimal Percentage { get; set; }
        public string Name { get; set; } = null!;
        public List<ExportSubmissionGrade> SubmissionGrades { get; set; } = new();
        public decimal? AssessmentAvg { get; set; }
        public int Order { get; set; }
    }
    public class ExportSubmissionGrade
    {
        public decimal Percentage { get; set; }
        public string Name { get; set; } = null!;
        public decimal? Grade { get; set; }
    }
}
