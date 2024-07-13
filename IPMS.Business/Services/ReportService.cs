using AutoMapper;
using IPMS.Business.Common.Constants;
using IPMS.Business.Common.Exceptions;
using IPMS.Business.Common.Utils;
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
        private readonly IPresignedUrlService _presignedUrlService;

        public ReportService(IUnitOfWork unitOfWork, IMapper mapper, IPresignedUrlService presignedUrlService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _presignedUrlService = presignedUrlService;
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

        public async Task<IEnumerable<StudentReportResponse>> GetStudentReport(StudentReportRequest request, Guid reporterId)
        {
            var searchValue = request.SearchValue;
            if (searchValue == null)
            {
                searchValue = "";
            }
            searchValue = searchValue.Trim().ToLower();

            IEnumerable<StudentReportResponse> reports = new List<StudentReportResponse>();
            var preReports = await _unitOfWork.ReportRepository.Get() // get report of current user with title or description contains searchValue
                            .Where(r => r.ReporterId.Equals(reporterId) 
                                && (r.Content.ToLower().Contains(searchValue) || r.Title.ToLower().Contains(searchValue)))
                            .Include(r => r.ReportType).ToListAsync();

            reports = preReports.Select(pr => new StudentReportResponse
            {
                Id = pr.Id,
                ReportType = pr.ReportType.Name,
                Title = pr.Title,
                Description = pr.Content,
                Response = pr.ResponseContent,
                Status = pr.Status,
                CreateAt = pr.CreatedAt,
                Detail = _presignedUrlService.GeneratePresignedDownloadUrl(S3KeyUtils.GetS3Key(S3KeyPrefix.Report,pr.Id,pr.FileName))

            });
            return reports;

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
