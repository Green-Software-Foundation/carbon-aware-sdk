using GSF.CarbonAware.Handlers;
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
        var locationSource = serviceProvider.GetService(typeof(ILocationHandler)) as ILocationHandler ?? throw new NullReferenceException("ILocationHandler not found");

        var locations = await locationSource.GetLocationsAsync();
        var serializedOuput = JsonSerializer.Serialize(locations);
        context.Console.WriteLine(serializedOuput);
        context.ExitCode = 0;
    }
}
