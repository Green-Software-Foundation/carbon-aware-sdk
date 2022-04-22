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
            return await this._dataSource.GetCarbonIntensityAsync(GetOrDefaultLocation(props), GetDefaultPeriodOrThrow(props), GetDefaultPeriodOrThrow(props, false));

        }

        private DateTimeOffset GetDefaultPeriodOrThrow(IDictionary props, bool isStart = true) {
            if (isStart) {
                var start = props[CarbonAwareConstants.Start];
                var startDate = DateTimeOffset.Now;
                if (start != null && !DateTimeOffset.TryParse(start.ToString(), out startDate))
                {
                    throw new ArgumentException("start period must be provided and must be a date time offset");
                }
            
                return startDate;
            } else {
                var end = props[CarbonAwareConstants.End];
                var endDate = DateTimeOffset.Now;
                if (end != null && !DateTimeOffset.TryParse(end.ToString(), out endDate))
                {
                    throw new ArgumentException("end period must be provided and must be a date time offset");
                }
            
                return endDate;
            }
        }

        private IEnumerable<Location> GetOrDefaultLocation(IDictionary props) {
            return props[CarbonAwareConstants.Locations] as IEnumerable<Location> ?? throw new ArgumentException("locations parameter must be provided and be non empty");
        }
    }
}