using IPMS.API.Common.Enums;
using IPMS.API.Common.Extensions;
using IPMS.API.Responses;
using IPMS.Business.Common.Enums;
using IPMS.Business.Interfaces.Services;

namespace IPMS.API.Middlewares
{
    public class CheckTokenRoleStillValidMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CheckTokenRoleStillValidMiddleware> _logger;

        public CheckTokenRoleStillValidMiddleware(RequestDelegate next,
        ILogger<CheckTokenRoleStillValidMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context, IAuthenticationService authenticationService)
        {
            var isTokenRoleValid = await authenticationService.CheckUserClaimsInTokenStillValidAsync(context.User.Claims);
            if(isTokenRoleValid)
            {
                // continue if token role still valid
                await _next(context);
            }
            else
            {
                var response = new IPMSResponse<object>();
                response.Status = ResponseStatus.Unauthorized;
                context.Response.StatusCode = (int)response.Status;
                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }
}
