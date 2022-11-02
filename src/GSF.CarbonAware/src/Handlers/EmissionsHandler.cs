using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Aggregators.Emissions;
using CarbonAware.Exceptions;
using CarbonAware.Tools.WattTimeClient;
using GSF.CarbonAware.Exceptions;
using Microsoft.Extensions.Logging;

namespace GSF.CarbonAware.Handlers;

internal sealed class EmissionsHandler : IEmissionsHandler
{
    private readonly ILogger<EmissionsHandler> _logger;
    private readonly IEmissionsAggregator _aggregator;

    public EmissionsHandler(ILogger<EmissionsHandler> logger, IEmissionsAggregator aggregator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _aggregator = aggregator ?? throw new ArgumentNullException(nameof(aggregator));
    }

    /// <inheritdoc />
    public async Task<double> GetAverageCarbonIntensityAsync(string location, DateTimeOffset start, DateTimeOffset end)
    {
        var parameters = new CarbonAwareParametersBaseDTO {
            Start = start,
            End = end,
            SingleLocation = location
        };
        try {
            var result = await _aggregator.CalculateAverageCarbonIntensityAsync(parameters);
            _logger.LogDebug("calculated average carbon intensity: {carbonIntensity}", result);
            return result;
        }
        catch (Exception ex) when (ex is WattTimeClientException || ex is WattTimeClientHttpException || ex is LocationConversionException || ex is global::CarbonAware.Tools.WattTimeClient.Configuration.ConfigurationException)
        {
            throw new CarbonIntensityException(ex.Message, ex);
        }
    }
}