namespace CarbonAware.CLI;

using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Aggregators.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

class Program
{
    public static async Task Main(string[] args)
    {   
        ICarbonAwareAggregator aggregator =  GetRequiredService();

        await GetEmissionsAsync(args, aggregator);
    }

    private static ICarbonAwareAggregator GetRequiredService() {
             
        var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();
        var config = configurationBuilder.Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(config);
        services.AddCarbonAwareEmissionServices();

        services.AddLogging();
        var serviceProvider = services.BuildServiceProvider();

        return serviceProvider.GetRequiredService<ICarbonAwareAggregator>();
    }
    private static async Task GetEmissionsAsync(string[] args, ICarbonAwareAggregator aggregator) {
        var cli = new CarbonAwareCLI(args, aggregator);

        if (cli.Parsed)
        {
            var emissions = await cli.GetEmissions();
            cli.OutputEmissionsData(emissions);
        }    
    }
}
