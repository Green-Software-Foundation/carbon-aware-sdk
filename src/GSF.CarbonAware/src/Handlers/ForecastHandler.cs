using CarbonAware;
using CarbonAware.Exceptions;
using CarbonAware.Extensions;
using CarbonAware.Interfaces;
using CarbonAware.Model;
using Microsoft.Extensions.Logging;
using static CarbonAware.Model.CarbonAwareParameters;
using EmissionsForecast = GSF.CarbonAware.Models.EmissionsForecast;

namespace GSF.CarbonAware.Handlers;

internal sealed class ForecastHandler : IForecastHandler
{
    private readonly IForecastDataSource _forecastDataSource;
    private readonly ILogger<ForecastHandler> _logger;

    /// <summary>
    /// Creates a new instance of the <see cref="ForecastHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger for the handler</param>
    /// <param name="datasource">An <see cref="IForecastDataSource"> datasource.</param>
    public ForecastHandler(ILogger<ForecastHandler> logger, IForecastDataSource dataSource)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _forecastDataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmissionsForecast>> GetCurrentForecastAsync(string[] locations, DateTimeOffset? start = null, DateTimeOffset? end = null, int? duration = null)
    {
        var dto = new CarbonAwareParametersBaseDTO
        {
            Start = start,
            End = end,
            MultipleLocations = locations,
            Duration = duration
        };

        var parameters = (CarbonAwareParameters) dto;
        try
        {
            parameters.SetRequiredProperties(PropertyName.MultipleLocations);
            parameters.Validate();
            var forecasts = new List<EmissionsForecast>();
            foreach (var location in parameters.MultipleLocations)
            {
                var forecast = await _forecastDataSource.GetCurrentCarbonIntensityForecastAsync(location);
                var emissionsForecast = ProcessAndValidateForecast(forecast, parameters);
                forecasts.Add(emissionsForecast);
            }
            return forecasts;
        }
        catch (CarbonAwareException ex)
        {
            throw new Exceptions.CarbonAwareException(ex.Message, ex);
        }
    }

    /// <inheritdoc />
    public async Task<EmissionsForecast> GetForecastByDateAsync(string location, DateTimeOffset? start = null, DateTimeOffset? end = null, DateTimeOffset? requestedAt = null, int? duration = null)
    {
        var dto = new CarbonAwareParametersBaseDTO
        {
            Start = start,
            End = end,
            SingleLocation = location,
            Requested = requestedAt,
            Duration = duration
        };

        var parameters = (CarbonAwareParameters) dto;
        try
        {
            parameters.SetRequiredProperties(PropertyName.SingleLocation, PropertyName.Requested);
            parameters.Validate();
            var forecast = await _forecastDataSource.GetCarbonIntensityForecastAsync(parameters.SingleLocation, parameters.Requested);
            var emissionsForecast = ProcessAndValidateForecast(forecast, parameters);
            return emissionsForecast;
        }
        catch (CarbonAwareException ex)
        {
            throw new Exceptions.CarbonAwareException(ex.Message, ex);
        }
    }

    private static EmissionsForecast ProcessAndValidateForecast(global::CarbonAware.Model.EmissionsForecast forecast, CarbonAwareParameters parameters)
    {
        var windowSize = parameters.Duration;
        var firstDataPoint = forecast.ForecastData.First();
        var lastDataPoint = forecast.ForecastData.Last();
        forecast.DataStartAt = parameters.GetStartOrDefault(firstDataPoint.Time);
        forecast.DataEndAt = parameters.GetEndOrDefault(lastDataPoint.Time + lastDataPoint.Duration);
        forecast.Validate();
        forecast.ForecastData = IntervalHelper.FilterByDuration(forecast.ForecastData, forecast.DataStartAt, forecast.DataEndAt);
        forecast.ForecastData = forecast.ForecastData.RollingAverage(windowSize, forecast.DataStartAt, forecast.DataEndAt);
        forecast.OptimalDataPoints = CarbonAwareOptimalEmission.GetOptimalEmissions(forecast.ForecastData);
        if (forecast.ForecastData.Any())
        {
            forecast.WindowSize = forecast.ForecastData.First().Duration;
        }
        return forecast;
    }
}
