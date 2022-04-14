namespace CarbonAwareCLI;

using CarbonAware.Plugins.JsonReaderPlugin.Configuration;
using Microsoft.Extensions.DependencyInjection;

class Program
{
    private readonly ICarbonAware _plugin;
    public Program(ICarbonAware plugin) {
        this._plugin = plugin;
    }
    public static void Main(string[] args)
    {   
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddCarbonAwareServices();
        serviceCollection.AddLogging();     
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var services = serviceProvider.GetServices<ICarbonAware>();
        
        //Currently there is just one implementation. This will have to change once we implement WattTime
        ICarbonAware _plugin = services.First();
        var cli = new CarbonAwareCLI(args, _plugin);

        if (cli.Parsed)
        {
            var emissions = cli.GetEmissions();
            cli.OutputEmissionsData(emissions.Result);
        }
    }
    public async Task GetEmissionsData(string[] args) {
        var cli = new CarbonAwareCLI(args, _plugin);

        if (cli.Parsed)
        {
            var emissions = await cli.GetEmissions();
            cli.OutputEmissionsData(emissions);
        }    
    }
}
