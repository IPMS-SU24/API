using IPMS.API.Filters;
using IPMS.API.Middlewares;
using IPMS.Business;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Repositories;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Repositories;
using IPMS.Business.Repository;
using IPMS.Business.Services;
using Microsoft.AspNetCore.Authorization;

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
            services.AddScoped<IReportTypeRepository, ReportTypeRepository>();
            services.AddScoped<IReportRepository, ReportRepository>();
            services.AddScoped<IMemberHistoryRepository, MemberHistoryRepository>();
            services.AddScoped<ILecturerGradeRepository, LecturerGradeRepository>();
            services.AddScoped<IFavoriteRepository, FavoriteRepository>();
            services.AddScoped<ITopicFavoriteRepository, TopicFavoriteRepository>();
            services.AddScoped<IClassModuleDeadlineRepository, ClassModuleDeadlineRepository>();
            services.AddScoped<ICommitteeRepository, CommitteeRepository>();
            services.AddScoped<IBasicIoTDeviceRepository, BasicIotDeviceRepository>();
            services.AddScoped<IIoTKitRepository, IoTKitRepository>();
            services.AddScoped<IKitProjectRepository, KitProjectRepository>();
            services.AddScoped<IKitDeviceRepository, KitDeviceRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            //Add Depenency Injection
            services.AddSingleton<IMessageService, MessageService>();
            services.AddSingleton<IPresignedUrlService, PresignedUrlService>();
            services.AddSingleton<IAuthorizationMiddlewareResultHandler, IPMSAuthorizationMiddlewareResultHandler>();

            //Add Service
            services.AddTransient<IBackgoundJobService,BackgroundJobService>();
            services.AddScoped<ITopicService, TopicService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IProjectDashboardService, ProjectDashboardService>();
            services.AddScoped<IClassTopicService, ClassTopicService>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<ICommonServices, CommonServices>();
            services.AddScoped<IAssessmentService, AssessmentService>();
            services.AddScoped<IBorrowIoTService, BorrowIoTService>();
            services.AddScoped<IProjectSubmissionService, ProjectSubmissionService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IStudentGroupService, StudentGroupService>();
            services.AddScoped<IIoTDataService, IoTDataService>();
            services.AddScoped<IMemberHistoryService, MemberHistoryService>();
            services.AddScoped<ISemesterService, SemesterService>();
            services.AddScoped<IClassService, ClassService>();
            services.AddScoped<ISubmissionModuleService, SubmissionModuleService>();
            services.AddScoped<INotificationStorageService, NotificationStorageService>();
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<IFavoriteTopicListService, FavoriteTopicListService>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<ApiExceptionFilterAttribute>();
        }
    }
}
