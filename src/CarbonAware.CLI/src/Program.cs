using CarbonAware;
using CarbonAware.Exceptions;
using CarbonAware.Aggregators.Configuration;
using CarbonAware.CLI.Commands.Emissions;
using CarbonAware.CLI.Commands.EmissionsForecasts;
using CarbonAware.CLI.Common;
using CarbonAware.CLI.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using CarbonAware.CLI.Commands.Location;

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
    .Build();

return await parser.InvokeAsync(args);
