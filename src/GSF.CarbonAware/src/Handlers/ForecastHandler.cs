using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Aggregators.Forecast;
using GSF.CarbonAware.Models;
using Microsoft.Extensions.Logging;
using CarbonAwareException = CarbonAware.Exceptions.CarbonAwareException;

namespace GSF.CarbonAware.Handlers;

internal sealed class ForecastHandler : IForecastHandler
{
    private readonly IForecastAggregator _aggregator;
    private readonly ILogger<ForecastHandler> _logger;

    /// <summary>
    /// Creates a new instance of the <see cref="ForecastHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger for the handler</param>
    /// <param name="aggregator">An <see cref="IForecastAggregator"> aggregator.</param>
    public ForecastHandler(ILogger<ForecastHandler> logger, IForecastAggregator aggregator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _aggregator = aggregator ?? throw new ArgumentNullException(nameof(aggregator));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmissionsForecast>> GetCurrentForecastAsync(string[] locations, DateTimeOffset? start = null, DateTimeOffset? end = null, int? duration = null)
    {
        var parameters = new CarbonAwareParametersBaseDTO
        {
            Start = start,
            End = end,
            MultipleLocations = locations,
            Duration = duration
        };
        try {
            var forecasts = await _aggregator.GetCurrentForecastDataAsync(parameters);
            var result = forecasts.Select(f => (EmissionsForecast)f);
            _logger.LogDebug("Current forecast: {result}", result);
            return result;
        }
        catch (CarbonAwareException ex)
        {
            throw new Exceptions.CarbonAwareException(ex.Message, ex);
        }
    }
}
