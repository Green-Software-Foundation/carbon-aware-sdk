using CarbonAware.Model;
using CarbonAware.Plugins;
using CarbonAware.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections;

namespace CarbonAware.Plugins.CarbonIntensity;

public class CarbonIntensityPlugin : ICarbonAware
{
    public string Name => "CarbonIntensityPlugin";

    public string Description => throw new NotImplementedException();

    public string Author => throw new NotImplementedException();

    public string Version => throw new NotImplementedException();

    private ILogger<CarbonIntensityPlugin> _logger { get; }

    private ICarbonIntensityDataSource _dataSource { get; }

    public CarbonIntensityPlugin(ILogger<CarbonIntensityPlugin> logger, ICarbonIntensityDataSource dataSource)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dataSource = dataSource;
    }

    public Task<IEnumerable<EmissionsData>> GetEmissionsDataAsync(IDictionary props)
    {
        throw new NotImplementedException();
    }
}