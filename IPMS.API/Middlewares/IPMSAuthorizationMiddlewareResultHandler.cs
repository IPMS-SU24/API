using IPMS.API.Common.Enums;
using IPMS.API.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace IPMS.API.Middlewares
{
    public class IPMSAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
    {
        private readonly IAuthorizationMiddlewareResultHandler _defaultHandler = new AuthorizationMiddlewareResultHandler();
        public async Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
        {
            var response = new IPMSResponse<object>();
            if (authorizeResult.Challenged)
            {
                response.Status = ResponseStatus.Unauthorized;
                context.Response.StatusCode = (int)response.Status;
                await context.Response.WriteAsJsonAsync(response);
                return;
            }
            if (authorizeResult.Forbidden)
            {
                response.Status = ResponseStatus.Forbidden;
                context.Response.StatusCode = (int)response.Status;
                await context.Response.WriteAsJsonAsync(response);
                return;
            }
            await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
        }
    }
}
