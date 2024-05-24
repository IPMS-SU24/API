using IPMS.Business.Models;
using IPMS.Business.Requests.Authentication;
using Microsoft.AspNetCore.Identity;

namespace IPMS.Business.Interfaces.Services
{
    public interface IAuthenticationService
    {
        Task<TokenModel?> Login(LoginRequest loginModel);
        Task<IdentityResult> AddLecturerAccount(AddLecturerAccountRequest registerModel);
        Task<TokenModel?> RefreshToken(TokenModel tokenModel);
    }
}
