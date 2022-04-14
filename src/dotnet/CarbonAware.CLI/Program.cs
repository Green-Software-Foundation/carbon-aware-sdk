namespace CarbonAwareCLI;

using CarbonAware.Plugins.JsonReaderPlugin.Configuration;
using Microsoft.Extensions.Hosting;

class Program
{
    
    private readonly ICarbonAware _plugin;
    public Program(ICarbonAware plugin) {
        this._plugin = plugin;
    }
    public static void Main(string[] args)
    {        
        var host = createHostBuilder(args).Build();
        host.Services.GetRequiredService<Program>().GetEmissionsData(args);
        
    }

    private static IHostBuilder createHostBuilder(string[] args) {
        return Host.CreateDefaultBuilder(args)
                        .ConfigureServices(services => {
                            services.AddCarbonAwareServices();
                            services.AddTransient<Program>();
                        });
    } 

    public void GetEmissionsData(string[] args) {
        var cli = new CarbonAwareCLI(args, _plugin);

        if (cli.Parsed)
        {
            var emissions = cli.GetEmissions();
            cli.OutputEmissionsData(emissions);
        }    
    }
}
