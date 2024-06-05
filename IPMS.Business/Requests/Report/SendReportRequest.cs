namespace IPMS.Business.Requests.Report
{
    public class SendReportRequest
    {
        public Guid ReportTypeId { get; set; }
        public string ReportTitle { get; set; } = null!;
        public string ReportDescription { get; set; } = null!;
        public string? FileName { get; set; }
    }
}
