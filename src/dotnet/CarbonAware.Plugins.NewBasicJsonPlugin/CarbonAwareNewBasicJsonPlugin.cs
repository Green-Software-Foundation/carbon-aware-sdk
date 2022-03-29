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
        // return await Task.Run(() => Enumerable.Empty<EmissionsData>());
        return await Task.Run(() => new List<EmissionsData>() {
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
        });
    }

    public async Task<IEnumerable<EmissionsData>> GetEmissionsDataAsync(IDictionary props, Func<QueryObject, bool> filter)
    {
        return await Task.Run(() => Enumerable.Empty<EmissionsData>());
    }
}
