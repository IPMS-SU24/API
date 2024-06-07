﻿using IPMS.Business.Requests.Report;
using IPMS.Business.Responses.Report;

namespace IPMS.Business.Interfaces.Services
{
    public interface IReportService
    {
        Task<IEnumerable<ReportTypeResponse>> GetReportType();
        Task SendReport(SendReportRequest request, Guid reporterId);
        Task<(bool result, string? message)> CheckValidReport(SendReportRequest request, Guid reporterId);
    }
}