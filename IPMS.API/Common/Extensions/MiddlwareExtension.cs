using IPMS.API.Middlewares;

namespace IPMS.API.Common.Extensions
{
    public static class MiddlwareExtension
    {
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
        }
    }
}
