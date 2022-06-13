using CarbonAware.Model;
using CarbonAware.Interfaces;
using CarbonAware.Extensions;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Diagnostics;

namespace CarbonAware.Aggregators.CarbonAware;

public class CarbonAwareAggregator : ICarbonAwareAggregator
{
    private static readonly ActivitySource Activity = new ActivitySource(nameof(CarbonAwareAggregator));
    private readonly ILogger<CarbonAwareAggregator> _logger;
    private readonly ICarbonIntensityDataSource _dataSource;

    /// <summary>
    /// Creates a new instance of the <see cref="CarbonAwareAggregator"/> class.
    /// </summary>
    /// <param name="logger">The logger for the aggregator</param>
    /// <param name="dataSource">An <see cref="ICarbonIntensityDataSource"> data source.</param>
    public CarbonAwareAggregator(ILogger<CarbonAwareAggregator> logger, ICarbonIntensityDataSource dataSource)
    {
        this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this._dataSource = dataSource;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmissionsData>> GetEmissionsDataAsync(IDictionary props)
    {
        using (var activity = Activity.StartActivity())
        {
            DateTimeOffset end = GetOffsetOrDefault(props, CarbonAwareConstants.End, DateTimeOffset.Now);
            DateTimeOffset start = GetOffsetOrDefault(props, CarbonAwareConstants.Start, end.AddDays(-7));
            _logger.LogInformation("Aggregator getting carbon intensity from data source");
            return await this._dataSource.GetCarbonIntensityAsync(GetLocationOrThrow(props), start, end);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmissionsForecast>> GetCurrentForecastDataAsync(IDictionary props)
    {
        using (var activity = Activity.StartActivity())
        {
            DateTimeOffset start = GetOffsetOrDefault(props, CarbonAwareConstants.Start, DateTimeOffset.Now);
            DateTimeOffset end = GetOffsetOrDefault(props, CarbonAwareConstants.End,  start.AddDays(1));
            TimeSpan windowSize = GetDurationOrDefault(props);
            _logger.LogInformation("Aggregator getting carbon intensity forecast from data source");

            var forecasts = new List<EmissionsForecast>();
            foreach(var location in GetLocationOrThrow(props))
            {
                var forecast = await this._dataSource.GetCurrentCarbonIntensityForecastAsync(location);
                forecast.StartTime = start;
                forecast.EndTime = end;
                forecast.ForecastData = IntervalHelper.FilterByDuration(forecast.ForecastData, start, end);
                forecast.ForecastData = forecast.ForecastData.RollingAverage(windowSize);
                if(forecast.ForecastData.Any())
                {
                    forecast.OptimalDataPoint = GetOptimalEmissions(forecast.ForecastData);
                    forecast.WindowSize = forecast.ForecastData.First().Duration;
                }
                forecasts.Add(forecast);
            }

            return forecasts;
        }
    }

    private EmissionsData GetOptimalEmissions(IEnumerable<EmissionsData> emissionsData)
    {
        return emissionsData.Aggregate((minData, nextData) => minData.Rating < nextData.Rating ? minData : nextData);
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

    private TimeSpan GetDurationOrDefault(IDictionary props, TimeSpan defaultValue = default)
    {
        if (props[CarbonAwareConstants.Duration] is int duration)
        {
            return TimeSpan.FromMinutes(duration);
        }
        return defaultValue;
    }

    public async Task<double> CalcEmissionsAverageAsync(IDictionary props)
    {
        ValidateAverageProps(props);
        var list = await GetEmissionsDataAsync(props);
        // check whether the list if empty, if not, return Rating's average, otherwise 0.
        var value = list.Any() ? list.Select(x => x.Rating).Average() : 0;
        _logger.LogInformation($"Carbon Intensity Average: {value}");
        return value;
    }

    private void ValidateAverageProps(IDictionary props)
    {
        if (!props.Contains(CarbonAwareConstants.Locations) ||
            !props.Contains(CarbonAwareConstants.Start) ||
            !props.Contains(CarbonAwareConstants.End))
        {
            throw new ArgumentException("Missing properties to calculate average");
        }
    }

}
