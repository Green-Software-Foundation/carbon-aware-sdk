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

    public async Task<IEnumerable<EmissionsData>> GetEmissionsDataAsync(Dictionary<string, object> props)
    {
        return await Task.Run(() => FakeData());
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
