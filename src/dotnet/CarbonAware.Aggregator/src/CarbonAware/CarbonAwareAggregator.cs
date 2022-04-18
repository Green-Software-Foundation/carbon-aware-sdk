using CarbonAware.Model;
using CarbonAware.Plugin;
using Microsoft.Extensions.Logging;
using System.Collections;

namespace CarbonAware.Aggregator.CarbonAware
{
    public class CarbonAwareAggregator : ICarbonAwareAggregator
    {
        private readonly ILogger<CarbonAwareAggregator> _logger;

        private readonly ICarbonAware _plugin;

        public CarbonAwareAggregator(ILogger<CarbonAwareAggregator> logger, ICarbonAware plugin)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._plugin = plugin;
        }

        public async Task<IEnumerable<EmissionsData>> GetEmissionsDataAsync(IDictionary props)
        {
            return await this._plugin.GetEmissionsDataAsync(props);

        }
    }
}