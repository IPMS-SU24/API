using IPMS.Business.Interfaces.Services;
using IPMS.Business.Services;

namespace IPMS.API.Common.Extensions
{
    public static class ServiceExtension
    {
        public static void AddDI(this IServiceCollection services)
        {
            //TODO in Sprint 2
            //Add Depenency Injection
            //Wait for init DB
            services.AddSingleton<IMessageService, MessageService>();
        }
    }
}
