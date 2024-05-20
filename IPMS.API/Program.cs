using AutoFilterer.Swagger;
using AutoMapper.Internal;
using FluentValidation;
using IPMS.API.Common.Extensions;
using IPMS.API.Filters;
using IPMS.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using System;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.Converters.Add(new StringEnumConverter());
    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
});
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
builder.Services.AddDbContext<IPMSDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("IPMS"), b=>b.MigrationsAssembly("IPMS.DataAccess")));
builder.Configuration.AddUserSecrets<IPMSDbContext>();
//TODO Config Add DbContext connection
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.DescribeAllParametersInCamelCase();
    options.UseAutoFiltererParameters();
});
builder.Services.AddSwaggerGenNewtonsoftSupport();
//TODO in Sprint 3
//Config JWT
//Add Identity Type
builder.Services.AddAutoMapper(cfg => cfg.Internal().MethodMappingEnabled = false, Assembly.GetExecutingAssembly());
var app = builder.Build();

// Configure the HTTP request pipeline.
 app.UseSwagger();
 app.UseSwaggerUI();
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseHsts();
}
app.UseHttpsRedirection();

app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.UseAuthorization();

app.MapControllers();

app.Run();
