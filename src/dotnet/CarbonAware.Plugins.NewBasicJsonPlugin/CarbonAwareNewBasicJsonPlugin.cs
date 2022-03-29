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
    public async Task<IEnumerable<EmissionsData>> GetEmissionsData(IDictionary props)
    {
        return await Task.Run(() => Enumerable.Empty<EmissionsData>());
    }

    public async Task<IEnumerable<EmissionsData>> GetEmissionsData(IDictionary props, Func<QueryObject, bool> filter)
    {
        return await Task.Run(() => Enumerable.Empty<EmissionsData>());
    }
}
