using IPMS.DataAccess.Common.Enums;

namespace IPMS.Business.Responses.Admin
{
    public class GetReportDetailResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Email { get; set; }
        public Guid TypeId { get; set; }
        public string Type { get; set; }
        public DateTime Date { get; set; }
        public RequestStatus Status { get; set; }
        public string Content { get; set; }
        public string? ResponseContent { get; set; }
        public string? ReportFile { get; set; }
    }
}
