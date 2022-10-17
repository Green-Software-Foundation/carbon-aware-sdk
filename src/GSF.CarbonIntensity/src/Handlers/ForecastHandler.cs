using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Model;
using GSF.CarbonIntensity.Model;
using Microsoft.Extensions.Logging;

namespace GSF.CarbonIntensity.Handlers;

internal sealed class ForecastHandler : IForecastHandler
{

    private readonly ICarbonAwareAggregator _aggregator;
    private readonly ILogger<ForecastHandler> _logger;

    public ForecastHandler(ICarbonAwareAggregator aggregator, ILogger<ForecastHandler> logger)
    {
        _aggregator = aggregator;
        _logger = logger;
    }

    public async Task<ForecastData> GetCurrent()
    {
        // TODO How to pass params.
        var param = new CarbonAwareParameters();
        var results = await _aggregator.GetCurrentForecastDataAsync(param);
        return CopyValues(results.First());
    }

    private ForecastData CopyValues(EmissionsForecast eForcast)
    {
        var data = new ForecastData();
        data.RequestedAt = eForcast.RequestedAt;
        data.GeneratedAt = eForcast.GeneratedAt;
        data.Location = eForcast.Location.ToString();
        data.DataStartAt = eForcast.DataStartAt;
        data.DataEndAt = eForcast.DataEndAt;
        data.WindowSize = eForcast.WindowSize;
        // TODO with the rest of properties. (Enumeration<EmissionsData>, Enumeration<OptimalData>)
        return data;
    }
}

