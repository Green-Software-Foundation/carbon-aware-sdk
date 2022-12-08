using CarbonAware;
using CarbonAware.Aggregators.Configuration;
using CarbonAware.CLI.Commands.Emissions;
using CarbonAware.CLI.Commands.EmissionsForecasts;
using CarbonAware.CLI.Common;
using CarbonAware.CLI.Extensions;
using CarbonAware.DataSources.ElectricityMaps;
using CarbonAware.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Diagnostics;


var config = new ConfigurationBuilder()
    .UseCarbonAwareDefaults()
    .Build();

var builder = new ServiceCollection()
    .AddSingleton<IConfiguration>(config)
    .Configure<CarbonAwareVariablesConfiguration>(
        config.GetSection(CarbonAwareVariablesConfiguration.Key))
    .AddLogging(builder => builder.AddDebug());

string? errorMessage = "";
bool successfulEmissionServices = builder.TryAddCarbonAwareEmissionServices(config, out errorMessage);

var serviceProvider = builder.BuildServiceProvider();

if(!successfulEmissionServices)
{
    var _logger = serviceProvider.GetService<ILogger<Program>>();
    _logger?.LogError(errorMessage);
}

var rootCommand = new RootCommand(description: CommonLocalizableStrings.RootCommandDescription);
rootCommand.AddGlobalOption(CommonOptions.VerboseOption);
rootCommand.AddCommand(new EmissionsCommand());
rootCommand.AddCommand(new EmissionsForecastsCommand());

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
