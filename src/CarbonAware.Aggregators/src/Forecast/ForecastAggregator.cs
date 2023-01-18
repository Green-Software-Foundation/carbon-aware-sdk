
using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Extensions;
using CarbonAware.Interfaces;
using CarbonAware.Model;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using static CarbonAware.Aggregators.CarbonAware.CarbonAwareParameters;

namespace CarbonAware.Aggregators.Forecast;
public class ForecastAggregator : IForecastAggregator
{
    private static readonly ActivitySource Activity = new(nameof(ForecastAggregator));
    private readonly ILogger<ForecastAggregator> _logger;
    private readonly IForecastDataSource _dataSource;

    /// <summary>
    /// Creates a new instance of the <see cref="ForecastAggregator"/> class.
    /// </summary>
    /// <param name="logger">The logger for the aggregator</param>
    /// <param name="dataSource">An <see cref="IForecastDataSource"> data source.</param>
    public ForecastAggregator(ILogger<ForecastAggregator> logger, IForecastDataSource dataSource)
    {
        this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this._dataSource = dataSource;
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
        forecast.OptimalDataPoints = GetOptimalEmissions(forecast.ForecastData);
        if (forecast.ForecastData.Any())
        {
            forecast.WindowSize = forecast.ForecastData.First().Duration;
        }
        return forecast;
    }

    private static IEnumerable<EmissionsData> GetOptimalEmissions(IEnumerable<EmissionsData> emissionsData)
    {
        if (!emissionsData.Any())
        {
            return Array.Empty<EmissionsData>();
        }

        var bestResult = emissionsData.MinBy(x => x.Rating);

        IEnumerable<EmissionsData> results = Array.Empty<EmissionsData>();

        if (bestResult != null)
        {
            results = emissionsData.Where(x => x.Rating == bestResult.Rating);
        }

        return results;
    }
}
