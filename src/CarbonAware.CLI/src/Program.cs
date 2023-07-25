using CarbonAware;
using CarbonAware.CLI.Commands.Emissions;
using CarbonAware.CLI.Commands.EmissionsForecasts;
using CarbonAware.CLI.Commands.Location;
using CarbonAware.CLI.Common;
using CarbonAware.CLI.Extensions;
using GSF.CarbonAware.Configuration;
using GSF.CarbonAware.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;

var config = new ConfigurationBuilder()
    .UseCarbonAwareDefaults()
    .Build();

var builder = new ServiceCollection()
    .AddSingleton<IConfiguration>(config)
    .Configure<CarbonAwareVariablesConfiguration>(
        config.GetSection(CarbonAwareVariablesConfiguration.Key))
    .AddLogging(builder => builder.AddDebug());

try
{
    builder.AddEmissionsServices(config);
}
catch (CarbonAwareException e)
{
    var _logger = builder.BuildServiceProvider().GetService<ILogger<Program>>();
    _logger?.LogError(e, "Failed to create emissions services.");
    Environment.Exit(1);
}

try
{
    builder.AddForecastServices(config);
}
catch (CarbonAwareException e)
{
    var _logger = builder.BuildServiceProvider().GetService<ILogger<Program>>();
    _logger?.LogError(e, "Failed to create forecast services.");
    Environment.Exit(1);
}

var serviceProvider = builder.BuildServiceProvider();

var rootCommand = new RootCommand(description: CommonLocalizableStrings.RootCommandDescription);
rootCommand.AddGlobalOption(CommonOptions.VerboseOption);
rootCommand.AddCommand(new EmissionsCommand());
rootCommand.AddCommand(new EmissionsForecastsCommand());
rootCommand.AddCommand(new LocationsCommand());

var parser = new CommandLineBuilder(rootCommand)
    .UseDefaults()
    .UseCarbonAwareExceptionHandler()
    .AddMiddleware(async (context, next) =>
        {
            context.BindingContext.AddService<IServiceProvider>(_ => serviceProvider);
            await next(context);
        }
    )
    .AddMiddleware(async (context, next) =>
        {
            if (context.ParseResult.HasOption(CommonOptions.VerboseOption)) 
            { 
                var serviceName = "CarbonAware.CLI";
                var serviceVersion = "1.0.0";

                using var tracerProvider = Sdk.CreateTracerProviderBuilder()
                    .AddSource(serviceName)
                    .SetResourceBuilder(ResourceBuilder
                        .CreateDefault()
                        .AddService(serviceName: serviceName, serviceVersion: serviceVersion))
                    .AddConsoleExporter()
                    .AddHttpClientInstrumentation()
                    .Build();
                await next(context);
            }
            else
            {
                await next(context);
            }
        }
    )
    .Build();


return await parser.InvokeAsync(args);
