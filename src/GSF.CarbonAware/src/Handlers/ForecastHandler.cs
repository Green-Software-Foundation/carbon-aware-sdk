using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Aggregators.Forecast;
using CarbonAware.Exceptions;
using CarbonAware.Tools.WattTimeClient;
using GSF.CarbonAware.Exceptions;
using GSF.CarbonAware.Models;
using Microsoft.Extensions.Logging;

namespace GSF.CarbonAware.Handlers;

internal sealed class ForecastHandler : IForecastHandler
{
    private readonly IForecastAggregator _aggregator;
    private readonly ILogger<ForecastHandler> _logger;

    public ForecastHandler(ILogger<ForecastHandler> logger, IForecastAggregator aggregator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _aggregator = aggregator ?? throw new ArgumentNullException(nameof(aggregator));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmissionsForecast>> GetCurrentAsync(string[] locations, DateTimeOffset? start = null, DateTimeOffset? end = null, int? duration = null)
    {
        var parameters = new CarbonAwareParametersBaseDTO
        {
            Start = start,
            End = end,
            MultipleLocations = locations,
            Duration = duration
        };
        try {
            var results = await _aggregator.GetCurrentForecastDataAsync(parameters);
            return results.Select(f => (EmissionsForecast) f);
        }
        catch (Exception ex) when (ex is WattTimeClientException || ex is WattTimeClientHttpException || ex is LocationConversionException || ex is global::CarbonAware.Tools.WattTimeClient.Configuration.ConfigurationException)
        {
            throw new CarbonIntensityException(ex.Message, ex);
        }
    }
}
