using CarbonAware.Extensions;
using CarbonAware.Interfaces;
using CarbonAware.Model;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using PropertyName = CarbonAware.Aggregators.CarbonAware.CarbonAwareParameters.PropertyName;

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
    public async Task<IEnumerable<EmissionsData>> GetEmissionsDataAsync(CarbonAwareParameters parameters)
    {
        using (var activity = Activity.StartActivity())
        {
            parameters.SetRequiredProperties(PropertyName.MultipleLocations);
            parameters.Validate();

            var locations = parameters.MultipleLocations;
            var end = parameters.GetEndOrDefault(DateTimeOffset.UtcNow);
            var start = parameters.GetStartOrDefault(end.AddDays(-7));
            
            return await this._dataSource.GetCarbonIntensityAsync(locations, start, end);
        }
    }

    /// <inheritdoc />
    public async Task<EmissionsData?> GetBestEmissionsDataAsync(CarbonAwareParameters parameters)
    {
        parameters.SetRequiredProperties(PropertyName.MultipleLocations);
        parameters.Validate();

        var locations = parameters.MultipleLocations;
        var end = parameters.GetEndOrDefault(DateTimeOffset.UtcNow);
        var start = parameters.GetStartOrDefault(end.AddDays(-7));

        var results = await this._dataSource.GetCarbonIntensityAsync(locations, start, end);
        return GetOptimalEmissions(results);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmissionsForecast>> GetCurrentForecastDataAsync(CarbonAwareParameters parameters)
    {
        using (var activity = Activity.StartActivity())
        {
            parameters.SetRequiredProperties(PropertyName.MultipleLocations);
            parameters.Validate();
            var forecasts = new List<EmissionsForecast>();
            foreach (var location in parameters.MultipleLocations)
            {
                var forecast = await this._dataSource.GetCurrentCarbonIntensityForecastAsync(location);
                var emissionsForecast = ProcessAndValidateForecast(forecast, parameters);
                forecasts.Add(emissionsForecast);
            }

            return forecasts;
        }
    }

    /// <inheritdoc />
    public async Task<double> CalculateAverageCarbonIntensityAsync(CarbonAwareParameters parameters)
    {
        using (var activity = Activity.StartActivity())
        {
            parameters.SetRequiredProperties(PropertyName.SingleLocation, PropertyName.Start, PropertyName.End);
            parameters.Validate();

            var end = parameters.End;
            var start = parameters.Start;

            _logger.LogInformation("Aggregator getting average carbon intensity from data source");
            var emissionData = await this._dataSource.GetCarbonIntensityAsync(parameters.SingleLocation, start, end);
            var value = emissionData.AverageOverPeriod(start, end);
            _logger.LogInformation($"Carbon Intensity Average: {value}");

            return value;
        }
    }

    public async Task<EmissionsForecast> GetForecastDataAsync(CarbonAwareParameters parameters)
    {
        EmissionsForecast forecast;
        using (var activity = Activity.StartActivity())
        {
            parameters.SetRequiredProperties(PropertyName.SingleLocation, PropertyName.Requested);
            parameters.Validate();

            forecast = await this._dataSource.GetCarbonIntensityForecastAsync(parameters.SingleLocation, parameters.Requested);
            var emissionsForecast = ProcessAndValidateForecast(forecast, parameters);
            return emissionsForecast;
        }
    }

    private static EmissionsForecast ProcessAndValidateForecast(EmissionsForecast forecast, CarbonAwareParameters parameters)
    {
        var windowSize = parameters.Duration;
        var firstDataPoint = forecast.ForecastData.First();
        var lastDataPoint = forecast.ForecastData.Last();
        forecast.DataStartAt = parameters.GetStartOrDefault(firstDataPoint.Time);
        forecast.DataEndAt = parameters.GetEndOrDefault(lastDataPoint.Time + lastDataPoint.Duration);
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

    private static EmissionsData? GetOptimalEmissions(IEnumerable<EmissionsData> emissionsData)
    {
        if (!emissionsData.Any())
        {
            return null;
        }
        return emissionsData.MinBy(x => x.Rating);
    }
}
