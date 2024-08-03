using IPMS.Business.Models;
using IPMS.Business.Requests.Admin;
using IPMS.Business.Requests.Authentication;
using IPMS.Business.Responses.Admin;
using IPMS.Business.Responses.Authentication;
using System.Security.Claims;

namespace IPMS.Business.Interfaces.Services
{
    public interface IAuthenticationService
    {
        Task<TokenModel?> Login(LoginRequest loginModel);
        Task AddLecturerAccount(AddLecturerAccountRequest registerModel);
        Task UpdateLecturerAccount(UpdateLecturerAccountRequest updateModel);
        Task<TokenModel?> RefreshToken(TokenModel tokenModel);
        Task ConfirmEmailAsync(Guid userId, string token);
        Task<bool> CheckUserClaimsInTokenStillValidAsync(IEnumerable<Claim> claims);
        Task ForgotPasswordAsync(ForgotPasswordRequest request);
        Task ResetPasswordAsync(ResetPasswordRequest request);
        Task ChangePasswordAsync(ChangePasswordRequest request, Guid userId);
        Task<IList<LectureAccountResponse>> GetLecturerAsync();
        Task<IEnumerable<LectureAccountResponse>> GetLecturerList(GetLecturerListRequest request);
        Task<GetLecturerDetailResponse> GetLecturerDetail(Guid lecturerId);
        Task<IEnumerable<GetAllStudentResponse>> GetAllStudent(GetAllStudentRequest request);
        Task<GetStudentDetailResponse> GetStudentDetail(Guid studentId);
        Task<IEnumerable<GetReportListResponse>> GetReportList(GetReportListRequest request);
        Task<GetReportDetailResponse> GetReportDetail(Guid? reportId);
        Task ResponseReport(ResponseReportRequest request);
        Task<IEnumerable<GetAllAssessmentResponse>> GetAllAssessment(GetAllAssessmentRequest request);
        Task<GetAssessmentDetailResponse> GetAssessmentDetail(Guid? assessmentId);
        Task<IEnumerable<GetAllSyllabusResponse>> GetAllSyllabus(GetAllSyllabusRequest request);
        Task<GetSyllabusDetailResponse> GetSyllabusDetail(Guid? syllabusId);

    }
}
