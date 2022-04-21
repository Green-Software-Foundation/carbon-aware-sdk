namespace CarbonAware.CLI;

using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Aggregators.Configuration;
using Microsoft.Extensions.DependencyInjection;

class Program
{
    public static async Task Main(string[] args)
    {   
        ICarbonAwareAggregator aggregator =  GetRequiredService();

        await GetEmissionsAsync(args, aggregator);
    }

    private static ICarbonAwareAggregator GetRequiredService() {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddCarbonAwareEmissionServices();
        serviceCollection.AddLogging();     
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var services = serviceProvider.GetServices<ICarbonAwareAggregator>();
        
        //Currently there is just one implementation. This will have to change once we implement WattTime
        return services.First();
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
