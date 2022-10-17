using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Model;
using GSF.CarbonIntensity.Models;
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

    public async Task<ForecastData> GetCurrent(CarbonAwareParameters parameters)
    {
        var results = await _aggregator.GetCurrentForecastDataAsync(parameters);
        return CopyValues(results.First());
    }

    private ForecastData CopyValues(EmissionsForecast eForcast)
    {
        var data = new ForecastData {
            RequestedAt = eForcast.RequestedAt,
            GeneratedAt = eForcast.GeneratedAt,
            Location = eForcast.Location.ToString(),
            DataStartAt = eForcast.DataStartAt,
            DataEndAt = eForcast.DataEndAt,
            WindowSize = eForcast.WindowSize
        };
        // TODO with the rest of properties. (Enumeration<EmissionsData>, Enumeration<OptimalData>)
        return data;
    }
}

