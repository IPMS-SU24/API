using AutoMapper;
using IPMS.Business.Common.Exceptions;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Models;
using IPMS.Business.Requests.Report;
using IPMS.Business.Responses.Report;
using IPMS.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace IPMS.Business.Services
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public ReportService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ValidationResultModel> CheckValidReport(SendReportRequest request, Guid reporterId)
        {
            var reportTypeMatch = await _unitOfWork.ReportTypeRepository.GetByIDAsync(request.ReportTypeId);
            if (reportTypeMatch == null) return new()
            {
                Result = false,
                Message = "Cannot found the Report Type"
            };
            return new()
            {
                Result = true
            };
        }

        public async Task<IEnumerable<ReportTypeResponse>> GetReportType()
        {
            var reportTypes = await _unitOfWork.ReportTypeRepository.Get().ToListAsync();
            if (!reportTypes.Any()) throw new DataNotFoundException();
            return _mapper.Map<IEnumerable<ReportTypeResponse>>(reportTypes);
        }

        public async Task SendReport(SendReportRequest request, Guid reporterId)
        {
            var reportForSave = _mapper.Map<Report>(request, opts =>
            {
                opts.Items["ReporterId"] = reporterId;
            });
            await _unitOfWork.ReportRepository.InsertAsync(reportForSave);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
