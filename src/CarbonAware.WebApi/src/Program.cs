using System.Reflection;
using CarbonAware;
using CarbonAware.Aggregators.Configuration;
using CarbonAware.WebApi.Filters;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
{
    options.Filters.Add<HttpResponseExceptionFilter>();
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var filePath = Path.Combine(System.AppContext.BaseDirectory, "CarbonAware.WebApi.xml");
    c.IncludeXmlComments(filePath);
    c.CustomOperationIds(apiDesc =>
       {
           return apiDesc.TryGetMethodInfo(out MethodInfo methodInfo) ? methodInfo.Name : null;
       });
});
builder.Services.Configure<CarbonAwareVariablesConfiguration>(builder.Configuration.GetSection(CarbonAwareVariablesConfiguration.Key));
builder.Services.AddCarbonAwareEmissionServices(builder.Configuration);
CarbonAwareVariablesConfiguration config = new CarbonAwareVariablesConfiguration();

builder.Configuration.GetSection(CarbonAwareVariablesConfiguration.Key).Bind(config);

builder.Services.AddHealthChecks();

var app = builder.Build();

if (config.WebApiRoutePrefix != null)
{
    app.UsePathBase(config.WebApiRoutePrefix);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();

// Please view https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-6.0#basic-tests-with-the-default-webapplicationfactory
// This line is needed to allow for Integration Testing
public partial class Program { }