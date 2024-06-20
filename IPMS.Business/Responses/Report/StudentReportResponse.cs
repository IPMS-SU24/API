using IPMS.DataAccess.Common.Enums;

namespace IPMS.Business.Responses.Report
{
    public class StudentReportResponse
    {
        public Guid Id { get; set; }
        public string ReportType { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Response { get; set; } = "";
        public RequestStatus Status { get; set; }
        public DateTime? CreateAt { get; set; }
    }
}
