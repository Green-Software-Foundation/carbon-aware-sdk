using CarbonAware.Aggregators;
using CarbonAware.Aggregators.CarbonAware;
using GSF.CarbonIntensity.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace GSF.CarbonIntensity.Handlers
{
    public class EmissionsHandler : IEmissionsHandler
    {
        private readonly ILogger<EmissionsHandler> _logger;
        private readonly ICarbonAwareAggregator _aggregator;

        public EmissionsHandler(ILogger<EmissionsHandler> logger, ICarbonAwareAggregator aggregator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _aggregator = aggregator ?? throw new ArgumentNullException(nameof(aggregator));
        }

        /// <inheritdoc />
        public async Task<CarbonIntensityResult> GetAverageCarbonIntensity(CarbonAwareParameters parameters)
        {
            var result = await _aggregator.CalculateAverageCarbonIntensityAsync(parameters);
            var carbonIntensity = new CarbonIntensityResult
            {
                Location = parameters.SingleLocation.DisplayName,
                StartTime = parameters.Start,
                EndTime = parameters.End,
                CarbonIntensity = result,
            };
            _logger.LogDebug("calculated average carbon intensity: {carbonIntensity}", carbonIntensity);
            return carbonIntensity;
        }
    }
}