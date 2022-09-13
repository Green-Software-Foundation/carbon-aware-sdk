using CarbonAware;
using CarbonAware.Aggregators.Configuration;
using CarbonAware.CLI.Commands.Emissions;
using CarbonAware.CLI.Common;
using CarbonAware.CLI.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;

var config = new ConfigurationBuilder()
    .UseCarbonAwareDefaults()
    .Build();

var serviceProvider = new ServiceCollection()
    .AddSingleton<IConfiguration>(config)
    .Configure<CarbonAwareVariablesConfiguration>(
        config.GetSection(CarbonAwareVariablesConfiguration.Key))
    .AddCarbonAwareEmissionServices(config)
    .AddLogging(builder => builder.AddDebug())
    .BuildServiceProvider();


var rootCommand = new RootCommand(description: CommonLocalizableStrings.RootCommandDescription);
rootCommand.AddCommand(new EmissionsCommand());

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
