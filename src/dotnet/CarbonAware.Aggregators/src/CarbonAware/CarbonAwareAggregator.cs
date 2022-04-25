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
            DateTimeOffset end = GetOffsetOrDefault(props, CarbonAwareConstants.End, DateTimeOffset.Now);
            DateTimeOffset start = GetOffsetOrDefault(props, CarbonAwareConstants.Start, end.AddDays(-7));
            _logger.LogInformation("Aggregator getting carbon intensity from data source");
            return await this._dataSource.GetCarbonIntensityAsync(GetLocationOrThrow(props), start, end);

        }

        /// <summary>
        /// Extracts the given offset prop and converts to DateTimeOffset. If prop is not defined, defaults to provided default
        /// </summary>
        /// <param name="props"></param>
        /// <returns>DateTimeOffset representing end period of carbon aware data search. </returns>
        /// <exception cref="ArgumentException">Throws exception if prop isn't a valid DateTimeOffset. </exception>
        private DateTimeOffset GetOffsetOrDefault(IDictionary props, string field, DateTimeOffset defaultValue) 
        {
            // Default if null
            var dateTimeOffset = props[field] ?? defaultValue;

            // If fail to parse property, throw error
            if (!DateTimeOffset.TryParse(dateTimeOffset.ToString(), out defaultValue))
            {
                Exception ex = new ArgumentException("Failed to parse" + field + "field. Must be a valid DateTimeOffset");
                _logger.LogError("argument exception", ex);
                throw ex;
            }

            return defaultValue;
        }

        private IEnumerable<Location> GetLocationOrThrow(IDictionary props) {
            if (props[CarbonAwareConstants.Locations] is IEnumerable<Location> locations)
            {
                return locations;
            }
            Exception ex = new ArgumentException("locations parameter must be provided and be non empty");
            _logger.LogError("argument exception", ex);
            throw ex;
        }
    }
}