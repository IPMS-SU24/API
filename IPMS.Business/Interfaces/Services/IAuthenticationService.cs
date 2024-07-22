using IPMS.Business.Models;
using IPMS.Business.Requests.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace IPMS.Business.Interfaces.Services
{
    public interface IAuthenticationService
    {
        Task<TokenModel?> Login(LoginRequest loginModel);
        Task<IdentityResult> AddLecturerAccount(AddLecturerAccountRequest registerModel);
        Task<TokenModel?> RefreshToken(TokenModel tokenModel);
        Task ConfirmEmailAsync(Guid userId, string token);
        Task<bool> CheckUserClaimsInTokenStillValidAsync(IEnumerable<Claim> claims);
        Task ForgotPasswordAsync(ForgotPasswordRequest request);
        Task ResetPasswordAsync(ResetPasswordRequest request);
        Task ChangePasswordAsync(ChangePasswordRequest request, Guid userId);
    }
}
