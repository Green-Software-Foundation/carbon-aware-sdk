namespace CarbonAware.CLI;

using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Aggregators.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

class Program
{
    public static async Task Main(string[] args)
    {
        ServiceProvider serviceProvider = BootstrapServices();

        await GetEmissionsAsync(args, serviceProvider.GetRequiredService<ICarbonAwareAggregator>(), serviceProvider.GetService<ILogger<CarbonAwareCLI>>());
    }

    private static ServiceProvider BootstrapServices() {
             
        var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();
        var config = configurationBuilder.Build();
        var services = new ServiceCollection();
        services.Configure<CarbonAwareVariablesConfiguration>(config.GetSection(CarbonAwareVariablesConfiguration.Key));
        services.AddSingleton<IConfiguration>(config);
        services.AddCarbonAwareEmissionServices(config);

        services.AddLogging(configure => configure.AddConsole());

        var serviceProvider = services.BuildServiceProvider();

        return serviceProvider;
    }

    private static async Task GetEmissionsAsync(string[] args, ICarbonAwareAggregator aggregator, ILogger<CarbonAwareCLI> logger) {
        var cli = new CarbonAwareCLI(args, aggregator, logger);

        if (cli.Parsed)
        {
            var emissions = await cli.GetEmissions();
            cli.OutputEmissionsData(emissions);
        }    
    }
}
