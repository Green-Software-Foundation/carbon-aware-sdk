using CarbonAware.Model;
using CarbonAware.Interfaces;
using Microsoft.Extensions.Logging;

namespace CarbonAware.DataSources.WattTime;

public class WattTimeDataSource : ICarbonIntensityDataSource
{
    public string Name => "WattTimeDataSource";

    public string Description => throw new NotImplementedException();

    public string Author => throw new NotImplementedException();

    public string Version => throw new NotImplementedException();

    private ILogger<WattTimeDataSource> _logger { get; }

    private ICarbonIntensityDataSource _dataSource { get; }

    public WattTimeDataSource(ILogger<WattTimeDataSource> logger, ICarbonIntensityDataSource dataSource)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dataSource = dataSource;
    }

    public Task<IEnumerable<EmissionsData>> GetCarbonIntensityAsync(IEnumerable<Location> locations, DateTimeOffset startPeriod, DateTimeOffset endPeriod)
    {
        throw new NotImplementedException();
    }
}