using IPMS.API.Common.Enums;
using IPMS.API.Responses;

namespace IPMS.API.Middlewares
{
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "An unexpected error occurred.");     

            var details = new IPMSResponse<object>()
            {
                Status = ResponseStatus.Fail
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)details.Status;
            await context.Response.WriteAsJsonAsync(details);
        }
    }
}
