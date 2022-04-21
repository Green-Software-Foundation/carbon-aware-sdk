namespace CarbonAware.CLI;

using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Aggregators.Configuration;
using Microsoft.Extensions.DependencyInjection;

class Program
{
    private static ICarbonAwareAggregator _aggregator;
    public static async Task Main(string[] args)
    {   
        InitializePlugin();

        await GetEmissionsData(args);
    }

    private static void InitializePlugin() {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddCarbonAwareEmissionServices();
        serviceCollection.AddLogging();     
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var services = serviceProvider.GetServices<ICarbonAwareAggregator>();
        
        //Currently there is just one implementation. This will have to change once we implement WattTime
        _aggregator = services.First();
    }
    private static async Task GetEmissionsData(string[] args) {
        var cli = new CarbonAwareCLI(args, _aggregator);

        if (cli.Parsed)
        {
            var emissions = await cli.GetEmissions();
            cli.OutputEmissionsData(emissions);
        }    
    }
}
