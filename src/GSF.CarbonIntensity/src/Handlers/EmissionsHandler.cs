using CarbonAware.Aggregators.CarbonAware;
using Microsoft.Extensions.Logging;

namespace GSF.CarbonIntensity.Handlers;

internal sealed class EmissionsHandler : IEmissionsHandler
{
    private readonly ILogger<EmissionsHandler> _logger;
    private readonly ICarbonAwareAggregator _aggregator;

    public EmissionsHandler(ILogger<EmissionsHandler> logger, ICarbonAwareAggregator aggregator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _aggregator = aggregator ?? throw new ArgumentNullException(nameof(aggregator));
    }

    /// <inheritdoc />
    public async Task<double> GetAverageCarbonIntensity(string location, DateTimeOffset start, DateTimeOffset end)
    {
        var parameters = new CarbonAwareParametersBaseDTO {
            Start = start,
            End = end,
            SingleLocation = location
        };
        var result = await _aggregator.CalculateAverageCarbonIntensityAsync(parameters);
        _logger.LogDebug("calculated average carbon intensity: {carbonIntensity}", result);
        return result;
    }
}