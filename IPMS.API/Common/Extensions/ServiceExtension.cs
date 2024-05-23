using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Repositories;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Repositories;
using IPMS.Business.Repository;
using IPMS.Business.Services;
using IPMS.Business;

namespace IPMS.API.Common.Extensions
{
    public static class ServiceExtension
    {
        public static void AddDI(this IServiceCollection services)
        {
            //Add Depenency Injection
            //Wait for init DB
            services.AddSingleton<IMessageService, MessageService>();

            //Add Repository
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<ISemesterRepository, SemesterRepository>();
            services.AddScoped<ITopicRepository, TopicRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            //Add Service
            services.AddScoped<ISemesterService, SemesterService>();
            services.AddScoped<ITopicService, TopicService>();
        }
    }
}
