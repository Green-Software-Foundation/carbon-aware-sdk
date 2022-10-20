using CarbonAware.Aggregators.CarbonAware;
using GSF.CarbonIntensity.Models;
using Microsoft.Extensions.Logging;

namespace GSF.CarbonIntensity.Handlers;

internal sealed class ForecastHandler : IForecastHandler
{
    private readonly ICarbonAwareAggregator _aggregator;
    private readonly ILogger<ForecastHandler> _logger;

    public ForecastHandler(ILogger<ForecastHandler> logger, ICarbonAwareAggregator aggregator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _aggregator = aggregator ?? throw new ArgumentNullException(nameof(aggregator));
    }

    public async Task<EmissionsForecast> GetCurrent(string location, DateTimeOffset start, DateTimeOffset end, int duration)
    {
        var parameters = new CarbonAwareParametersBaseDTO {
            Start = start,
            End = end,
            SingleLocation = location,
            Duration = duration
        };
        var results = await _aggregator.GetCurrentForecastDataAsync(parameters);
        return ToForecastData(results.First());
    }

    private static EmissionsForecast ToForecastData(CarbonAware.Model.EmissionsForecast emissionsForecast) {
        return new EmissionsForecast {
            RequestedAt = emissionsForecast.RequestedAt,
            GeneratedAt = emissionsForecast.GeneratedAt,
            EmissionsData = emissionsForecast.ForecastData.Select(x => ToEmissionsData(x)),
            OptimalDataPoints = emissionsForecast.OptimalDataPoints.Select(x => ToEmissionsData(x))
        };
    }

    private static EmissionsData ToEmissionsData(CarbonAware.Model.EmissionsData emissionsData)
    {
        return new EmissionsData {
                Duration = emissionsData.Duration,
                Location = emissionsData.Location,
                Rating = emissionsData.Rating,
                Time = emissionsData.Time
        };
    }
}
