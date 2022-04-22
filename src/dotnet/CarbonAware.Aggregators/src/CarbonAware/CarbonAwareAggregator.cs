using CarbonAware.Model;
using CarbonAware.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections;

namespace CarbonAware.Aggregators.CarbonAware
{
    public class CarbonAwareAggregator : ICarbonAwareAggregator
    {
        private readonly ILogger<CarbonAwareAggregator> _logger;

        private readonly ICarbonIntensityDataSource _dataSource;

        public CarbonAwareAggregator(ILogger<CarbonAwareAggregator> logger, ICarbonIntensityDataSource dataSource)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._dataSource = dataSource;
        }

        public async Task<IEnumerable<EmissionsData>> GetEmissionsDataAsync(IDictionary props)
        {
            DateTimeOffset end = GetEndOrDefaultToNow(props);
            DateTimeOffset start = GetStartOrDefaultToWeek(props, end);
            return await this._dataSource.GetCarbonIntensityAsync(GetOrDefaultLocation(props), start, end);

        }

        private DateTimeOffset GetStartOrDefaultToWeek(IDictionary props, DateTimeOffset endOffset) {
            var start = props[CarbonAwareConstants.Start];
            var startDate = endOffset.AddDays(-7);

            // If null, default to start period at a week before end
            if (start == null) {
                return startDate;
            }
            // If fail to parse property, throw error
            if (!DateTimeOffset.TryParse(start.ToString(), out startDate))
            {
                throw new ArgumentException("Failed to parse start period. Must be a valid DateTimeOffset");
            }
        
            return startDate;
        }

        private DateTimeOffset GetEndOrDefaultToNow(IDictionary props) {
            var end = props[CarbonAwareConstants.End];
            var endDate = DateTimeOffset.Now;

            // If null, default to end period at now
            if (end == null) {
                return endDate;
            }
            // If fail to parse property, throw error
            if (!DateTimeOffset.TryParse(end.ToString(), out endDate))
            {
                throw new ArgumentException("Failed to parse end period. Must be a valid DateTimeOffset");
            }
        
            return endDate;
        }

        private IEnumerable<Location> GetOrDefaultLocation(IDictionary props) {
            return props[CarbonAwareConstants.Locations] as IEnumerable<Location> ?? throw new ArgumentException("locations parameter must be provided and be non empty");
        }
    }
}