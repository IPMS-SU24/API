using IPMS.Business.Common.Singleton;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Repositories;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Repositories;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Repository;
using IPMS.Business.Repositories;
using IPMS.Business.Repository;
using IPMS.Business.Services;
using IPMS.Business;
using IPMS.Business.Repositories;
using IPMS.Business;
using Microsoft.AspNetCore.Authorization;
using IPMS.API.Middlewares;

namespace IPMS.API.Common.Extensions
{
    public static class ServiceExtension
    {
        public static void AddDI(this IServiceCollection services)
        {
            //Add Repository
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IComponentsMasterRepository, ComponentsMasterRepository>();
            services.AddScoped<IIoTComponentRepository, IoTComponentRepository>();
            services.AddScoped<IClassTopicRepository, ClassTopicRepository>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IProjectSubmissionRepository, ProjectSubmissionRepository>();
            services.AddScoped<IStudentRepository, StudentRepository>();
            services.AddScoped<ISubmissionModuleRepository, SubmissionModuleRepository>();
            services.AddScoped<IIPMSClassRepository, IPMSClassRepository>();
            services.AddScoped<ISemesterRepository, SemesterRepository>();
            services.AddScoped<ISyllabusRepository, SyllabusRepository>();
            services.AddScoped<IAssessmentRepository, AssessmentRepository>();
            services.AddScoped<ITopicRepository, TopicRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            //Add Depenency Injection
            services.AddSingleton<IMessageService, MessageService>();
            services.AddSingleton<IAuthorizationMiddlewareResultHandler, IPMSAuthorizationMiddlewareResultHandler>();

            //Add Service
            services.AddScoped<ITopicService, TopicService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IProjectDashboardService, ProjectDashboardService>();
            services.AddScoped<IClassTopicService, ClassTopicService>();
        }
    }
}
