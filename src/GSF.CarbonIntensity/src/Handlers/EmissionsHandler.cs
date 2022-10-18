using CarbonAware.Aggregators;
using CarbonAware.Aggregators.CarbonAware;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace GSF.CarbonIntensity.Handlers
{
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
        public async Task<double> GetAverageCarbonIntensity(CarbonAwareParameters parameters)
        {
            var result = await _aggregator.CalculateAverageCarbonIntensityAsync(parameters);
            _logger.LogDebug("calculated average carbon intensity: {carbonIntensity}", result);
            return result;
        }
    }
}