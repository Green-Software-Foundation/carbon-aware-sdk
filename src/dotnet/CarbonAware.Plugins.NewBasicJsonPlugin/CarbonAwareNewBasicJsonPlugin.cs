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
    Task<IEnumerable<EmissionsData>> ICarbonAware.GetEmissionsData(IDictionary props)
    {
        throw new NotImplementedException();
    }

    Task<IEnumerable<EmissionsData>> ICarbonAware.GetEmissionsData(IDictionary props, Func<QueryObject, bool> filter)
    {
        throw new NotImplementedException();
    }
}
