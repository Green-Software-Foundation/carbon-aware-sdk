using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Exceptions;
using CarbonAware.Tools.WattTimeClient;
using CarbonAware.Tools.WattTimeClient.Configuration;
using GSF.CarbonIntensity.Exceptions;
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

    public async Task<EmissionsForecast?> GetCurrentAsync(string location, DateTimeOffset? start, DateTimeOffset? end, int? duration)
    {
        var parameters = new CarbonAwareParametersBaseDTO
        {
            Start = start,
            End = end,
            MultipleLocations = new [] { location },
            Duration = duration
        };
        try {
            var results = await _aggregator.GetCurrentForecastDataAsync(parameters);
            return !results.Any() ? null : ToForecastData(results.First());
        }
        catch (Exception ex) when (ex is WattTimeClientException || ex is WattTimeClientHttpException || ex is LocationConversionException || ex is ConfigurationException)
        {
            throw new CarbonIntensityException(ex.Message, ex);
        }
    }

    private static EmissionsForecast ToForecastData(CarbonAware.Model.EmissionsForecast emissionsForecast) {
        return new EmissionsForecast {
            RequestedAt = emissionsForecast.RequestedAt,
            GeneratedAt = emissionsForecast.GeneratedAt,
            EmissionsDataPoints = emissionsForecast.ForecastData.Select(x => ToEmissionsData(x)),
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
