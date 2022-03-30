using System.Collections;
using CarbonAware.Model;
using Microsoft.Extensions.Logging;

namespace CarbonAware.Plugins.NewBasicJsonPlugin;

public class CarbonAwareNewBasicJsonPlugin : ICarbonAware
{
    private readonly ILogger<CarbonAwareNewBasicJsonPlugin> Logger;

    public CarbonAwareNewBasicJsonPlugin(ILogger<CarbonAwareNewBasicJsonPlugin> logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<EmissionsData>> GetEmissionsDataAsync(IDictionary props)
    {
        Logger.LogInformation("New Data");
        foreach (var k in props.Keys)
        {
            Logger.LogInformation($"Key: {k} - Value: {props[k]}");
        }
        return await Task.Run(() => FakeData());
    }

    public async Task<IEnumerable<EmissionsData>> GetEmissionsDataAsync(IDictionary props, Func<QueryObject, bool> filter)
    {
        return await Task.Run(() => FakeData().Where((x) => filter(new QueryObject { 
            Location = x.Location,
            Time = x.Time,
            Rating = x.Rating
        })));
    }

    private IEnumerable<EmissionsData> FakeData()
    {
        return  new List<EmissionsData>() {
            new EmissionsData {
                Location = "eastus",
                Time = DateTime.Now,
                Rating = 10
            },
            new EmissionsData {
                Location = "eastus",
                Time = new DateTime(2018, 09, 09),
                Rating = 2

            }
        };
    }
}
