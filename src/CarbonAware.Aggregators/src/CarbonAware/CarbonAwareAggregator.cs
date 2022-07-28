using CarbonAware.Extensions;
using CarbonAware.Interfaces;
using CarbonAware.Model;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Diagnostics;
using System.Globalization;

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
            DateTimeOffset end = GetOffsetOrDefault(props, CarbonAwareConstants.End, DateTimeOffset.Now.ToUniversalTime());
            DateTimeOffset start = GetOffsetOrDefault(props, CarbonAwareConstants.Start, end.AddDays(-7));
            _logger.LogInformation("Aggregator getting carbon intensity from data source");
            return await this._dataSource.GetCarbonIntensityAsync(GetLocationOrThrow(props), start, end);
        }
    }

    /// <inheritdoc />
    public async Task<EmissionsData?> GetBestEmissionsDataAsync(IDictionary props)
    {
        var results = await GetEmissionsDataAsync(props);
        return GetOptimalEmissions(results);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmissionsForecast>> GetCurrentForecastDataAsync(IDictionary props)
    {
        using (var activity = Activity.StartActivity())
        {
            _logger.LogInformation("Aggregator getting carbon intensity forecast from data source");

            var forecasts = new List<EmissionsForecast>();
            foreach (var location in GetLocationOrThrow(props))
            {
                var forecast = await this._dataSource.GetCurrentCarbonIntensityForecastAsync(location);
                var emissionsForecast = ProcessAndValidateForecast(forecast, props);
                forecasts.Add(emissionsForecast);
            }

            return forecasts;
        }
    }

    /// <inheritdoc />
    public async Task<double> CalculateAverageCarbonIntensityAsync(IDictionary props)
    {
        using (var activity = Activity.StartActivity())
        {
            var start = GetOffsetOrThrow(props, CarbonAwareConstants.Start);
            var end = GetOffsetOrThrow(props, CarbonAwareConstants.End);
            _logger.LogInformation("Aggregator getting average carbon intensity from data source");
            var emissionData = await this._dataSource.GetCarbonIntensityAsync(GetLocationOrThrow(props), start, end);
            var value = emissionData.AverageOverPeriod(start, end);
            _logger.LogInformation($"Carbon Intensity Average: {value}");

            return value;
        }
    }

    public async Task<EmissionsForecast> GetForecastDataAsync(IDictionary props)
    {
        EmissionsForecast forecast;
        using (var activity = Activity.StartActivity())
        {
            ValidateInput(props);

            var locations = (IEnumerable<Location>) props[CarbonAwareConstants.Locations]!;
            var forecastRequestedAt = GetOffsetOrDefault(props, CarbonAwareConstants.ForecastRequestedAt, default);
            _logger.LogDebug($"Aggregator getting carbon intensity forecast from data source for location {locations.First()} and requestedAt {forecastRequestedAt}");

            forecast = await this._dataSource.GetCarbonIntensityForecastAsync(locations.First(), forecastRequestedAt);
            var emissionsForecast = ProcessAndValidateForecast(forecast, props);
            return emissionsForecast;
        }
    }
    private void ValidateInput(IDictionary props)
    {
        var error = new ArgumentException("Invalid EmissionsForecast request");
        if (props[CarbonAwareConstants.Locations] == null)
        {
            error.Data["location"] = @"locations parameter must be provided and be non empty";
        }
        else if (props[CarbonAwareConstants.Locations] is IEnumerable<Location> locations && locations.Count() > 1)
        {
            error.Data["location"] = @"field should only contain one location for forecast data.";
        }
        if (props[CarbonAwareConstants.ForecastRequestedAt] == null)
        {
            error.Data["requestedAt"] = $"{CarbonAwareConstants.ForecastRequestedAt} field is required and was not provided.";
        }
        if (error.Data.Count > 0)
        {
            throw error;
        }
    }

    private EmissionsForecast ProcessAndValidateForecast(EmissionsForecast forecast, IDictionary props)
    {
        var windowSize = GetDurationOrDefault(props);
        var firstDataPoint = forecast.ForecastData.First();
        var lastDataPoint = forecast.ForecastData.Last();
        forecast.DataStartAt = GetOffsetOrDefault(props, CarbonAwareConstants.Start, firstDataPoint.Time);
        forecast.DataEndAt = GetOffsetOrDefault(props, CarbonAwareConstants.End, lastDataPoint.Time + lastDataPoint.Duration);
        forecast.Validate();
        forecast.ForecastData = IntervalHelper.FilterByDuration(forecast.ForecastData, forecast.DataStartAt, forecast.DataEndAt);
        forecast.ForecastData = forecast.ForecastData.RollingAverage(windowSize, forecast.DataStartAt, forecast.DataEndAt);
        forecast.OptimalDataPoint = GetOptimalEmissions(forecast.ForecastData);
        if (forecast.ForecastData.Any())
        {
            forecast.WindowSize = forecast.ForecastData.First().Duration;
        }
        return forecast;
    }

    private EmissionsData? GetOptimalEmissions(IEnumerable<EmissionsData> emissionsData)
    {
        if (!emissionsData.Any())
        {
            return null;
        }
        return emissionsData.MinBy(x => x.Rating);
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
        DateTimeOffset outValue;
        // If fail to parse property, throw error
        if (!DateTimeOffset.TryParse(dateTimeOffset.ToString(), null, DateTimeStyles.AssumeUniversal, out outValue))
        {
            Exception ex = new ArgumentException("Failed to parse" + field + "field. Must be a valid DateTimeOffset");
            _logger.LogError("argument exception", ex);
            throw ex;
        }

        return outValue;
    }

    private IEnumerable<Location> GetLocationOrThrow(IDictionary props)
    {
        if (props[CarbonAwareConstants.Locations] is IEnumerable<Location> locations)
        {
            return locations;
        }
        Exception ex = new ArgumentException("locations parameter must be provided and be non empty");
        _logger.LogError("argument exception", ex);
        throw ex;
    }

    /// <summary>
    /// Extracts the given offset prop and converts to DateTimeOffset. If prop is not defined, throws
    /// </summary>
    /// <param name="props"></param>
    /// <returns>DateTimeOffset representing end period of carbon aware data search. </returns>
    /// <exception cref="ArgumentException">Throws exception if prop isn't found or isn't a valid DateTimeOffset. </exception>
    private DateTimeOffset GetOffsetOrThrow(IDictionary props, string field)
    {
        if (props[field] != null)
        {
            return GetOffsetOrDefault(props, field, DateTimeOffset.MinValue);
        }

        Exception ex = new ArgumentException("Failed to find" + field + "field. Must be a valid DateTimeOffset");
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
}
