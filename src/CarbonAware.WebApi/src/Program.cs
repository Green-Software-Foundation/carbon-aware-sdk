using CarbonAware;
using CarbonAware.WebApi.Configuration;
using CarbonAware.WebApi.Filters;
using GSF.CarbonAware.Configuration;
using GSF.CarbonAware.Exceptions;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

// Define constants to initialize tracing with
var serviceName = "CarbonAware.WebAPI";
var serviceVersion = "1.0.0";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetryTracing(tracerProviderBuilder =>
{
    tracerProviderBuilder
        .AddConsoleExporter()
        .AddSource(serviceName)
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(serviceName: serviceName, serviceVersion: serviceVersion))
        .AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation();
});

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
    c.EnableAnnotations();
    c.OperationFilter<CarbonAwareParametersBaseDtoOperationFilter>();
    c.SchemaFilter<CarbonAwareParametersBaseDtoSchemaFilter>();
});

builder.Services.Configure<CarbonAwareVariablesConfiguration>(builder.Configuration.GetSection(CarbonAwareVariablesConfiguration.Key));

bool successfulServices = true;
string? errorMessage = null;
try
{
    builder.Services.AddEmissionsServices(builder.Configuration);
    builder.Services.AddForecastServices(builder.Configuration);
} catch(CarbonAwareException e)
{
    successfulServices = false;
    errorMessage = e.Message;
}

CarbonAwareVariablesConfiguration config = new();

builder.Configuration.GetSection(CarbonAwareVariablesConfiguration.Key).Bind(config);

builder.Services.AddHealthChecks();

builder.Services.AddMonitoringAndTelemetry(builder.Configuration);

builder.Services.AddSwaggerGen(c => {
        c.MapType<TimeSpan>(() => new OpenApiSchema { Type = "string", Format = "time-span" });
    });

var app = builder.Build();
 
if (!successfulServices)
{
    var _logger = app.Services.GetService<ILogger<Program>>();
    _logger?.LogError(errorMessage);
}

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