using GSF.CarbonAware.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration((context, builder) => {
        var env = context.HostingEnvironment;
        builder.AddJsonFile(Path.Combine(env.ContentRootPath, "appsettings.json"), optional: true, reloadOnChange: false)
               .AddJsonFile(Path.Combine(env.ContentRootPath, $"appsettings.{env.EnvironmentName}.json"), optional: true, reloadOnChange: false)
               .AddEnvironmentVariables();
    })
    .ConfigureServices((context,services) => {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddEmissionsServices(context.Configuration);
        services.AddForecastServices(context.Configuration);
    })
    .Build();

host.Run();