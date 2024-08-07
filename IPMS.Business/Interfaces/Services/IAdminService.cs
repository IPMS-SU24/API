using IPMS.Business.Models;
using IPMS.Business.Requests.Admin;
using IPMS.Business.Requests.Assessment;
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
        Task CloneSyllabus(CloneSyllabusRequest request);
        Task<IEnumerable<GetAllSemesterAdminResponse>> GetAllSemesterAdmin(GetAllSemesterAdminRequest request);
        Task<GetSemesterDetailResponse> GetSemesterDetail(Guid? semesterId);
        Task<ValidationResultModel> CreateSemesterValidators(CreateSemesterRequest request);
        Task CreateSemester(CreateSemesterRequest request);
        Task<ValidationResultModel> UpdateSemesterValidators(UpdateSemesterRequest request);
        Task UpdateSemester(UpdateSemesterRequest request);
        Task DeleteSemester(Guid? semesterId);



    }
}
