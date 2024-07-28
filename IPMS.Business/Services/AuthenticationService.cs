using Azure.Communication.Email;
using Azure;
using IPMS.Business.Common.Enums;
using IPMS.Business.Common.Exceptions;
using IPMS.Business.Common.Models;
using IPMS.Business.Common.Utils;
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
using IPMS.Business.Interfaces;
using IPMS.Business.Responses.Authentication;
using IPMS.Business.Requests.Admin;
using IPMS.Business.Responses.Admin;
using Microsoft.EntityFrameworkCore;

namespace IPMS.Business.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<IPMSUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly ICommonServices _commonService;
        private readonly MailServer _mailServer;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly JWTConfig _jwtConfig;
        private readonly string _mailHost;
        private readonly IUnitOfWork _unitOfWork;
        public AuthenticationService(UserManager<IPMSUser> userManager,
                                   RoleManager<IdentityRole<Guid>> roleManager,
                                   IOptions<JWTConfig> jwtConfig,
                                   ILogger<AuthenticationService> logger,
                                   ICommonServices commonService,
                                   MailServer mailServer,
                                   IConfiguration configuration,
                                   IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtConfig = jwtConfig.Value;
            _logger = logger;
            _commonService = commonService;
            _mailServer = mailServer;
            _mailHost = configuration["MailFrom"];
            _unitOfWork = unitOfWork;
        }

        public async Task AddLecturerAccount(AddLecturerAccountRequest registerModel)
        {
            await _unitOfWork.RollbackTransactionOnFailAsync(async () =>
            {

                var newLecturer = new IPMSUser
                {
                    Email = registerModel.Email,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = registerModel.Email.Split("@").First(),
                    PhoneNumber = registerModel.Phone,
                    FullName = registerModel.FullName
                };
                var password = PasswordGeneratorUtils.GenerateRandomPassword();
                var rs = await _userManager.CreateAsync(newLecturer, password);
                if (rs.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync(UserRole.Lecturer.ToString())) await _roleManager.CreateAsync(new IdentityRole<Guid>(UserRole.Lecturer.ToString()));
                    await _userManager.AddToRoleAsync(newLecturer, UserRole.Lecturer.ToString());
                    //Send mail Confirm
                    var confirmEmailToken = await _userManager.GenerateEmailConfirmationTokenAsync(newLecturer);
                    var confirmURL = PathUtils.GetConfirmURL(newLecturer.Id, confirmEmailToken);
                    EmailSendOperation emailSendOperation = await _mailServer.Client.SendAsync(
                                WaitUntil.Started,
                                _mailHost,
                                registerModel.Email,
                                ConfirmEmailTemplate.Subject,
                                EmailUtils.GetFullMailContent(ConfirmEmailTemplate.GetBody(confirmURL, password)));
                }
                else
                {
                    throw new ValidationException(rs.Errors.Select(x=> new FluentValidation.Results.ValidationFailure
                    {
                        PropertyName = x.Code,
                        ErrorMessage = x.Description
                    }));
                }
            });
        }
        public async Task<IList<LectureAccountResponse>> GetLecturerAsync()
        {
            return (await _userManager.GetUsersInRoleAsync(UserRole.Lecturer.ToString())).Select(x => new LectureAccountResponse
            {
                Id = x.Id,
                Name = x.FullName,
            }).ToList();
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
            if (user != null && !user.IsDeleted.Value && await _userManager.CheckPasswordAsync(user, loginModel.Password) && await _userManager.IsEmailConfirmedAsync(user))
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var authClaims = await GetUserClaims(user);
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
                await _commonService.SetCommonSessionUserEntity(user.Id);

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

            if (user == null || oldActiveRefreshToken == null || user.IsDeleted.Value)
            {
                return null;
            }
            //Revoke current token
            oldActiveRefreshToken.Revoked = DateTime.Now;
            var newAccessToken = GenerateAccessToken(await GetUserClaims(user));
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshTokens.Add(new UserRefreshToken
            {
                Token = refreshToken,
                Expires = DateTime.Now.AddDays(_jwtConfig.RefreshTokenValidityInDays)
            });
            await _userManager.UpdateAsync(user);
            await _commonService.SetCommonSessionUserEntity(user.Id);
            return new TokenModel
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }
        public async Task ConfirmEmailAsync(Guid userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if(user == null)
            {
                throw new DataNotFoundException();
            }
            if(await _userManager.IsEmailConfirmedAsync(user))
            {
                throw new EmailConfirmException("Email is already confirmed");
            }
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                throw new EmailConfirmException("Token is invalid");
            }
        }

        public async Task<bool> CheckUserClaimsInTokenStillValidAsync(IEnumerable<Claim> claims)
        {
            //No need to check for anonymous user
            if (!claims.Any()) return true;
            var user = await _userManager.FindByEmailAsync(claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)!.Value);
            var userRoles = await _userManager.GetRolesAsync(user);
            var roleInToken = claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToList();
            // userRole must contain all roleInToken and same length
            return !userRoles.Any(x => !roleInToken.Contains(x)) && userRoles.Count == roleInToken.Count;
        }

        private async Task<IList<Claim>> GetUserClaims(IPMSUser user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            var isFirstLogin = !user.RefreshTokens.Any();
            return new List<Claim>
                    {
                        new (ClaimTypes.Email, user.Email),
                        new (ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new ("Id", user.UserName),
                        new ("FullName", user.FullName),
                        new ("isFirstLogin", isFirstLogin.ToString(), ClaimValueTypes.Boolean),
                        new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new (ClaimTypes.Role, JsonSerializer.Serialize(userRoles), JsonClaimValueTypes.JsonArray)
                    };
        }

        public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if(user == null || !await _userManager.IsEmailConfirmedAsync(user))
            {
                throw new DataNotFoundException("Not Found Account");
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetUrl = PathUtils.GetResetPasswordURL(user.Id, token);
            EmailSendOperation emailSendOperation = await _mailServer.Client.SendAsync(
                           WaitUntil.Started,
                           _mailHost,
                           request.Email,
                           ForgotPasswordEmailTemplate.Subject,
                           EmailUtils.GetFullMailContent(ForgotPasswordEmailTemplate.GetBody(resetUrl)));
        }

        public async Task ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
            {
                throw new DataNotFoundException("Not Found Account");
            }
            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            if (!result.Succeeded)
            {
                throw new CannotResetPasswordException(result.Errors.Select(x => x.Description).ToArray());
            }
        }

        public async Task ChangePasswordAsync(ChangePasswordRequest request, Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user != null && !user.IsDeleted.Value)
            {
                var result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
                if (!result.Succeeded)
                {
                    throw new CannotResetPasswordException(result.Errors.Select(x => x.Description).ToArray());
                }
            }
        }

        public async Task<IEnumerable<LectureAccountResponse>> GetLecturerList(GetLecturerListRequest request)
        {
            var lecturers = (await _userManager.GetUsersInRoleAsync(UserRole.Lecturer.ToString())).Select(x => new LectureAccountResponse
            {
                Id = x.Id,
                Name = x.FullName,
                Email = x.Email
            }).ToList();
            if (request.Name != null && request.Email != null)
            {

                lecturers = lecturers.Where(l => l.Name.ToLower().Contains(request.Name.ToLower()) || l.Email.ToLower().Contains(request.Email.ToLower())).ToList();
            }
            else if (request.Name != null)
            {
                lecturers = lecturers.Where(l => l.Name.ToLower().Contains(request.Name.ToLower())).ToList();

            }
            else if (request.Email != null)
            {
                lecturers = lecturers.Where(l => l.Email.ToLower().Contains(request.Email.ToLower())).ToList();

            }


            return lecturers;
        }

        public async Task<GetLecturerDetailResponse> GetLecturerDetail(Guid lecturerId)
        {
            var lecturerRaw = await _userManager.FindByIdAsync(lecturerId.ToString());
            var classes = await _unitOfWork.IPMSClassRepository.Get().Where(c => c.LecturerId.Equals(lecturerId)).Select(c => new ClassInfoLecDetail
            {
                ClassId = c.Id,
                Name = c.Name,
                ShortName = c.ShortName
            }).ToListAsync();

            return new GetLecturerDetailResponse
            {
                Id = lecturerRaw.Id,
                Name = lecturerRaw.FullName,
                Email = lecturerRaw.Email,
                Classes = classes
            };
        }
    }
}
