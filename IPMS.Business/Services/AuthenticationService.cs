using IPMS.Business.Common.Enums;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Models;
using IPMS.Business.Requests.Authentication;
using IPMS.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace IPMS.Business.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<IPMSUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly ICommonServices _commonService;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly JWTConfig _jwtConfig;
        public AuthenticationService(UserManager<IPMSUser> userManager,
                                   RoleManager<IdentityRole<Guid>> roleManager,
                                   IOptions<JWTConfig> jwtConfig,
                                   ILogger<AuthenticationService> logger,
                                   ICommonServices commonService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtConfig = jwtConfig.Value;
            _logger = logger;
            _commonService = commonService;
        }

        public Task<IdentityResult> AddLecturerAccount(AddLecturerAccountRequest registerModel)
        {
            throw new NotImplementedException();
        }

        public string GenerateAccessToken(IEnumerable<Claim> claims)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Secret));
            var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
            var tokeOptions = new JwtSecurityToken(
                issuer: _jwtConfig.ValidIssuer,
                audience: _jwtConfig.ValidAudience,
                claims: claims,
                expires: DateTime.Now.AddHours(_jwtConfig.TokenExpiryTimeInHour),
                signingCredentials: signinCredentials
            );
            return new JwtSecurityTokenHandler().WriteToken(tokeOptions);
        }
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Secret)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

                if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                    throw new SecurityTokenException("Invalid token");

                return principal;
            }
            catch (ArgumentException ex)
            {
                _logger.LogInformation(ex, "User Token Validate Fail");
                return null;
            }

        }

        public async Task<TokenModel?> Login(LoginRequest loginModel)
        {
            var user = await _userManager.FindByEmailAsync(loginModel.Email);
            if (user != null && !user.IsDeleted && await _userManager.CheckPasswordAsync(user, loginModel.Password) && await _userManager.IsEmailConfirmedAsync(user))
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var authClaims = new List<Claim>
                    {
                        new (ClaimTypes.Email, user.Email),
                        new (ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new ("Id", user.UserName),
                        new ("FullName", user.FullName),
                        new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new (ClaimTypes.Role, JsonSerializer.Serialize(userRoles), JsonClaimValueTypes.JsonArray)
                    };
                if (userRoles.Contains(UserRole.Student.ToString()))
                {
                    //If student => Add ProjectId to Claim
                    var project = await _commonService.GetProject(user.Id);
                    if(project != null)
                    {
                        authClaims.Add(new("ProjectId",project.Id.ToString()));
                    }
                }
                var accessToken = GenerateAccessToken(authClaims);

                var refreshToken = string.Empty;

                if (user.RefreshTokens.Any(a => a.IsActive))
                {
                    var activeRefreshToken = user.RefreshTokens.Where(a => a.IsActive).FirstOrDefault();
                    refreshToken = activeRefreshToken.Token;
                }
                else
                {
                    refreshToken = GenerateRefreshToken();
                    user.RefreshTokens.Add(new UserRefreshToken
                    {
                        Token = refreshToken,
                        Expires = DateTime.Now.AddDays(_jwtConfig.RefreshTokenValidityInDays)
                    });
                    await _userManager.UpdateAsync(user);
                }

                return new TokenModel
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                };
            }
            return null;
        }

        public async Task<TokenModel?> RefreshToken(TokenModel tokenModel)
        {
            if (tokenModel is null)
            {
                return null;
            }

            string? accessToken = tokenModel.AccessToken;
            string? refreshToken = tokenModel.RefreshToken;

            var principal = GetPrincipalFromExpiredToken(accessToken);
            if (principal == null)
            {
                return null;
            }
            string email = principal.FindFirst(x=>x.Type == ClaimTypes.Email).Value;

            var user = await _userManager.FindByEmailAsync(email);
            var oldActiveRefreshToken = user.RefreshTokens.Where(x => x.IsActive && x.Token == refreshToken).FirstOrDefault();

            if (user == null || oldActiveRefreshToken == null || user.IsDeleted)
            {
                return null;
            }
            //Revoke current token
            oldActiveRefreshToken.Revoked = DateTime.Now;
            var newAccessToken = GenerateAccessToken(principal.Claims.ToList());
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshTokens.Add(new UserRefreshToken
            {
                Token = refreshToken,
                Expires = DateTime.Now.AddDays(_jwtConfig.RefreshTokenValidityInDays)
            });
            await _userManager.UpdateAsync(user);

            return new TokenModel
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }
    }
}
