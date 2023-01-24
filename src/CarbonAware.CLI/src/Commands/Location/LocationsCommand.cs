using CarbonAware.Interfaces;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json;

namespace CarbonAware.CLI.Commands.Location;

public class LocationsCommand : Command
{
    public LocationsCommand() : base("locations", LocalizableStrings.LocationsCommandDescription)
    {
        this.SetHandler(this.Run);
    }

    internal async Task Run(InvocationContext context)
    {
        var serviceProvider = context.BindingContext.GetService(typeof(IServiceProvider)) as IServiceProvider ?? throw new NullReferenceException("ServiceProvider not found");
        var locationSource = serviceProvider.GetService(typeof(ILocationSource)) as ILocationSource ?? throw new NullReferenceException("ILocationSource not found");

        var locations = await locationSource.GetGeopositionLocationsAsync();
        var serializedOuput = JsonSerializer.Serialize(locations);
        context.Console.WriteLine(serializedOuput);
        context.ExitCode = 0;
    }
}
