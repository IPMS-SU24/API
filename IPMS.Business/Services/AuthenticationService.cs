using IPMS.Business.Interfaces.Services;
using IPMS.Business.Models;
using IPMS.Business.Requests.Authentication;
using IPMS.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace IPMS.Business.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<IPMSUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthenticationService> _logger;
        public AuthenticationService(UserManager<IPMSUser> userManager,
                                   RoleManager<IdentityRole<Guid>> roleManager,
                                   IConfiguration configuration,
                                   ILogger<AuthenticationService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
        }

        public Task<IdentityResult> AddLecturerAccount(AddLecturerAccountRequest registerModel)
        {
            throw new NotImplementedException();
        }

        public string GenerateAccessToken(IEnumerable<Claim> claims)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["IPMS_JWT_Secret"]));
            var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
            var tokeOptions = new JwtSecurityToken(
                issuer: _configuration["IPMS_JWT_ValidIssuer"],
                audience: _configuration["IPMS_JWT_ValidAudience"],
                claims: claims,
                expires: DateTime.Now.AddHours(double.Parse(_configuration["IPMS_JWT_TokenExpiryTimeInHour"])),
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
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["IPMS_JWT_Secret"])),
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
            if (user != null && !user.IsDeleted && await _userManager.CheckPasswordAsync(user, loginModel.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var authClaims = new List<Claim>
                    {
                        new (ClaimTypes.Email, user.Email),
                        new (ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new ("Id", user.UserName),
                        new ("FullName", user.FullName),
                        new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var accessToken = GenerateAccessToken(authClaims);

                _ = int.TryParse(_configuration["IPMS_JWT_RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);
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
                        Expires = DateTime.Now.AddDays(refreshTokenValidityInDays)
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
            string username = principal.Identity.Name;

            var user = await _userManager.FindByNameAsync(username);
            var oldActiveRefreshToken = user.RefreshTokens.Where(x => x.IsActive && x.Token == refreshToken).FirstOrDefault();

            if (user == null || oldActiveRefreshToken == null || user.IsDeleted)
            {
                return null;
            }
            //Revoke current token
            oldActiveRefreshToken.Revoked = DateTime.Now;
            var newAccessToken = GenerateAccessToken(principal.Claims.ToList());
            var newRefreshToken = GenerateRefreshToken();

            _ = int.TryParse(_configuration["IPMS_JWT_RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);
            user.RefreshTokens.Add(new UserRefreshToken
            {
                Token = refreshToken,
                Expires = DateTime.Now.AddDays(refreshTokenValidityInDays)
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
