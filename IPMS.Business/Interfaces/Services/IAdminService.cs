using IPMS.Business.Models;
using IPMS.Business.Requests.Admin;
using IPMS.Business.Responses.Admin;
using IPMS.Business.Responses.Authentication;

namespace IPMS.Business.Interfaces.Services
{
    public interface IAdminService
    {
        Task<IList<LectureAccountResponse>> GetLecturerAsync();
        Task<IEnumerable<LectureAccountResponse>> GetLecturerList(GetLecturerListRequest request);
        Task<GetLecturerDetailResponse> GetLecturerDetail(Guid lecturerId);
        Task<IEnumerable<GetAllStudentResponse>> GetAllStudent(GetAllStudentRequest request);
        Task<GetStudentDetailResponse> GetStudentDetail(Guid studentId);
        Task<IEnumerable<GetReportListResponse>> GetReportList(GetReportListRequest request);
        Task<GetReportDetailResponse> GetReportDetail(Guid? reportId);
        Task ResponseReport(ResponseReportRequest request);
        Task<GetAssessmentDetailResponse> GetAssessmentDetail(Guid? assessmentId);
        Task<IEnumerable<GetAllSyllabusResponse>> GetAllSyllabus(GetAllSyllabusRequest request);
        Task<GetSyllabusDetailResponse> GetSyllabusDetail(Guid? syllabusId);
        Task<ValidationResultModel> UpdateSyllabusValidators(UpdateSyllabusRequest request);
        Task UpdateSyllabus(UpdateSyllabusRequest request);
        Task CloneSyllabus(Guid? syllabusId);
    }
}
