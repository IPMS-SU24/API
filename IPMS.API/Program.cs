using Amazon.S3;
using Amazon.SQS;
using AutoFilterer.Swagger;
using AutoMapper.Internal;
using FluentValidation;
using IPMS.API.Common;
using IPMS.API.Common.Extensions;
using IPMS.API.Filters;
using IPMS.Business.Common.Extensions;
using IPMS.Business.Models;
using IPMS.DataAccess;
using IPMS.DataAccess.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Console;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddControllers(options =>
{
    options.Conventions.Add(new RouteTokenTransformerConvention(
                                 new SlugifyParameterTransformer()));
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});
builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.RequestPath | HttpLoggingFields.RequestBody | HttpLoggingFields.RequestQuery;
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;
});
builder.Configuration.AddAmazonSecretsManager("ap-southeast-1", "env");
builder.Services.AddFluentValidationAutoValidation(option =>
{
    option.OverrideDefaultResultFactoryWith<IPMSResultFactory>();
});
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
});
builder.Services.AddDI();
builder.Services.AddDbContext<IPMSDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("IPMS"), b => b.MigrationsAssembly("IPMS.DataAccess"));
});
builder.Configuration.AddUserSecrets<IPMSDbContext>();
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddIdentity<IPMSUser, IdentityRole<Guid>>(config =>
{
    config.SignIn.RequireConfirmedEmail = false;
    config.SignIn.RequireConfirmedPhoneNumber = false;
}).AddEntityFrameworkStores<IPMSDbContext>()
            .AddDefaultTokenProviders();
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonSQS>();
builder.Services.AddAWSService<IAmazonS3>();
builder.Services.Configure<JWTConfig>(builder.Configuration.GetSection("JWT"));
builder.Services.AddSwaggerGen(options =>
{
    options.DescribeAllParametersInCamelCase();
    options.UseAutoFiltererParameters();
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
// Adding Jwt Bearer
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero,
        ValidAudience = builder.Configuration.GetSection("JWT")["ValidAudience"],
        ValidIssuer = builder.Configuration.GetSection("JWT")["ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("JWT")["Secret"]))
    };
});
builder.Services.AddAutoMapper(cfg => cfg.Internal().MethodMappingEnabled = false, Assembly.GetExecutingAssembly());
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var app = builder.Build();
app.UseGlobalExceptionHandling();
// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseCors(options => options.AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true) // allow any origin
                .AllowCredentials());
app.UseHttpLogging();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
