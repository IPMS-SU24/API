using IPMS.API.Common.Extensions;
using IPMS.Business.Common.Enums;
using IPMS.Business.Interfaces.Services;

namespace IPMS.API.Middlewares
{
    public class AddStudentSessionIfNotExistMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AddStudentSessionIfNotExistMiddleware> _logger;

        public AddStudentSessionIfNotExistMiddleware(RequestDelegate next,
        ILogger<AddStudentSessionIfNotExistMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context, ICommonServices commonServices)
        {
            context.Session.Clear();
            if (context.User != null && context.User.IsInRole(UserRole.Student.ToString()) && commonServices.GetProject() == null && commonServices.GetClass() == null)
            {
                commonServices.SetCommonSessionUserEntity(context.User.Claims.GetUserId()).GetAwaiter().GetResult();
            }
            await _next(context);
        }
    }
}
