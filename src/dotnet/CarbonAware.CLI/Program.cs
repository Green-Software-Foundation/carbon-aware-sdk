namespace CarbonAware.CLI;

using CarbonAware.Plugins.JsonReaderPlugin.Configuration;
using Microsoft.Extensions.DependencyInjection;

class Program
{
    private static ICarbonAware Plugin;
    public static void Main(string[] args)
    {   
        InitializePlugin();

        var cli = new CarbonAwareCLI(args, Plugin);

        if (cli.Parsed)
        {
            var emissions = cli.GetEmissions();
            cli.OutputEmissionsData(emissions.Result);
        }
    }

    private static void InitializePlugin() {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddCarbonAwareServices();
        serviceCollection.AddLogging();     
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var services = serviceProvider.GetServices<ICarbonAware>();
        
        //Currently there is just one implementation. This will have to change once we implement WattTime
        Plugin = services.First();
    }
    private async Task GetEmissionsData(string[] args) {
        var cli = new CarbonAwareCLI(args, Plugin);

        if (cli.Parsed)
        {
            var emissions = await cli.GetEmissions();
            cli.OutputEmissionsData(emissions);
        }    
    }
}
