using CarbonAware.DataSources.WattTime.Client;
using CarbonAware.DataSources.WattTime.Model;
using CarbonAware.Exceptions;
using CarbonAware.Interfaces;
using CarbonAware.Model;
using Microsoft.Extensions.Logging;

namespace CarbonAware.DataSources.WattTime;

/// <summary>
/// Represents a WattTime data source.
/// </summary>
internal class WattTimeDataSource : IEmissionsDataSource, IForecastDataSource
{
    public string Name => "WattTimeDataSource";

    public string Description => throw new NotImplementedException();

    public string Author => throw new NotImplementedException();

    public string Version => throw new NotImplementedException();

    private ILogger<WattTimeDataSource> Logger { get; }

    private IWattTimeClient WattTimeClient { get; }

    private ILocationSource LocationSource { get; }

    const double MWH_TO_KWH_CONVERSION_FACTOR = 1000.0;
    const double LBS_TO_GRAMS_CONVERSION_FACTOR = 453.59237;
    public double MinSamplingWindow => 120; // 2hrs of data

    /// <summary>
    /// Creates a new instance of the <see cref="WattTimeDataSource"/> class.
    /// </summary>
    /// <param name="logger">The logger for the datasource</param>
    /// <param name="client">The WattTime Client</param>
    /// <param name="locationSource">The location source to be used to convert a location to named region's.</param>
    public WattTimeDataSource(ILogger<WattTimeDataSource> logger, IWattTimeClient client, ILocationSource locationSource)
    {
        this.Logger = logger;
        this.WattTimeClient = client;
        this.LocationSource = locationSource;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmissionsData>> GetCarbonIntensityAsync(IEnumerable<Location> locations, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime)
    {
        this.Logger.LogInformation("Getting carbon intensity for locations {locations} for period {periodStartTime} to {periodEndTime}.", locations, periodStartTime, periodEndTime);
        List<EmissionsData> result = new();
        foreach (var location in locations)
        {
            IEnumerable<EmissionsData> interimResult = await GetCarbonIntensityAsync(location, periodStartTime, periodEndTime);
            result.AddRange(interimResult);
        }
        return result;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmissionsData>> GetCarbonIntensityAsync(Location location, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime)
    {
        Logger.LogInformation($"Getting carbon intensity for location {location} for period {periodStartTime} to {periodEndTime}.");
        var region = await this.GetRegion(location);
        var (newStartTime, newEndTime) = IntervalHelper.ExtendTimeByWindow(periodStartTime, periodEndTime, MinSamplingWindow);
        var historicalResponse = await this.WattTimeClient.GetDataAsync(region, newStartTime, newEndTime);
        if (Logger.IsEnabled(LogLevel.Debug))
        {
            Logger.LogDebug($"Found {historicalResponse.Data.Count()} total forecasts for location {location} for period {periodStartTime} to {periodEndTime}.");
        }
        var windowData = ConvertToEmissionsData(historicalResponse);
        var filteredData = IntervalHelper.FilterByDuration(windowData, periodStartTime, periodEndTime);

        if (!filteredData.Any())
        {
            Logger.LogInformation($"Not enough data with {MinSamplingWindow} window");
        }
        return filteredData;
    }

    /// <inheritdoc />
    public async Task<EmissionsForecast> GetCurrentCarbonIntensityForecastAsync(Location location)
    {
        this.Logger.LogInformation($"Getting carbon intensity forecast for location {location}");
        var region = await this.GetRegion(location);
        var forecast = await this.WattTimeClient.GetCurrentForecastAsync(region);
        return ForecastToEmissionsForecast(forecast, location, DateTimeOffset.UtcNow);
    }

    /// <inheritdoc />
    public async Task<EmissionsForecast> GetHistoricalCarbonIntensityForecastAsync(Location location, DateTimeOffset requestedAt)
    {
        this.Logger.LogInformation($"Getting carbon intensity forecast for location {location} requested at {requestedAt}");
        var region = await this.GetRegion(location);
        var roundedRequestedAt = TimeToLowestIncrement(requestedAt);
        var forecast = await this.WattTimeClient.GetForecastOnDateAsync(region, roundedRequestedAt);
        if (forecast == null)
        {
            var ex = new ArgumentException($"No forecast was generated at the requested time {roundedRequestedAt}");
            Logger.LogError(ex, ex.Message);
            throw ex;
        }
        // keep input from the user.
        return HistoricalForecastToEmissionsForecast(forecast, location, requestedAt);
    }

    private EmissionsForecast HistoricalForecastToEmissionsForecast(HistoricalForecastEmissionsDataResponse historicalForecast, Location location, DateTimeOffset requestedAt)
    {
        var duration = GetDurationFromGridEmissionDataPoints(historicalForecast.Data[0].Forecast);
        var forecastData = historicalForecast.Data[0].Forecast.Select(e => new EmissionsData()
        {
            Location = historicalForecast.Meta.Region,
            Rating = ConvertMoerToGramsPerKilowattHour(e.Value),
            Time = e.PointTime,
            Duration = duration
        });
        var emissionsForecast = new EmissionsForecast()
        {
            GeneratedAt = historicalForecast.Data[0].GeneratedAt,
            Location = location,
            ForecastData = forecastData
        };
        return emissionsForecast;
    }

    private EmissionsForecast ForecastToEmissionsForecast(ForecastEmissionsDataResponse forecast, Location location, DateTimeOffset requestedAt)
    {
        var duration = GetDurationFromGridEmissionDataPoints(forecast.Data);
        var forecastData = forecast.Data.Select(e => new EmissionsData()
        {
            Location = forecast.Meta.Region,
            Rating = ConvertMoerToGramsPerKilowattHour(e.Value),
            Time = e.PointTime,
            Duration = duration
        });
        var emissionsForecast = new EmissionsForecast()
        {
            GeneratedAt = forecast.Meta.GeneratedAt,
            Location = location,
            ForecastData = forecastData
        };
        return emissionsForecast;
    }

    internal double ConvertMoerToGramsPerKilowattHour(double value)
    {
        return value * LBS_TO_GRAMS_CONVERSION_FACTOR / MWH_TO_KWH_CONVERSION_FACTOR;
    }

    private IEnumerable<EmissionsData> ConvertToEmissionsData(GridEmissionsDataResponse gridEmissionDataPoints)
    {
        var defaultDuration = GetDurationFromGridEmissionDataPointsOrDefault(gridEmissionDataPoints.Data, TimeSpan.Zero);

        // Linq statement to convert WattTime forecast data into EmissionsData for the CarbonAware SDK.
        return gridEmissionDataPoints.Data.Select(e => new EmissionsData()
        {
            Location = gridEmissionDataPoints.Meta.Region,
            Rating = ConvertMoerToGramsPerKilowattHour(e.Value),
            Time = e.PointTime,
            Duration = FrequencyToTimeSpanOrDefault(e.Frequency, defaultDuration)
        });
    }

    private TimeSpan GetDurationFromGridEmissionDataPoints(IEnumerable<GridEmissionDataPoint> gridEmissionDataPoints)
    {
        var firstPoint = gridEmissionDataPoints.FirstOrDefault();
        var secondPoint = gridEmissionDataPoints.Skip(1)?.FirstOrDefault();

        var first = firstPoint ?? throw new WattTimeClientException("Too few data points returned");
        var second = secondPoint ?? throw new WattTimeClientException("Too few data points returned");

        // Handle chronological and reverse-chronological data by using `.Duration()` to get
        // the absolute value of the TimeSpan between the two points.
        return first.PointTime.Subtract(second.PointTime).Duration();
    }

    private TimeSpan GetDurationFromGridEmissionDataPointsOrDefault(IEnumerable<GridEmissionDataPoint> gridEmissionDataPoints, TimeSpan defaultValue)
    {
        try
        {
            return GetDurationFromGridEmissionDataPoints(gridEmissionDataPoints);
        }
        catch (WattTimeClientException)
        {
            return defaultValue;
        }
    }

    private TimeSpan FrequencyToTimeSpanOrDefault(int? frequency, TimeSpan defaultValue)
    {
        return (frequency != null) ? TimeSpan.FromSeconds((double)frequency) : defaultValue;
    }

    private async Task<RegionResponse> GetRegion(Location location)
    {
        RegionResponse region;
        try
        {
            var geolocation = await this.LocationSource.ToGeopositionLocationAsync(location);
            region = await WattTimeClient.GetRegionAsync(geolocation.LatitudeAsCultureInvariantString(), geolocation.LongitudeAsCultureInvariantString());
        }
        catch (Exception ex) when (ex is LocationConversionException || ex is WattTimeClientHttpException)
        {
            Logger.LogError(ex, "Failed to convert the location {location} into a Region.", location);
            throw;
        }

        Logger.LogDebug("Converted location {location} to region {region}", location, region.Region);

        return region;
    }

    private DateTimeOffset TimeToLowestIncrement(DateTimeOffset date, int minutes = 5)
    {
        var d = TimeSpan.FromMinutes(minutes);
        return new DateTimeOffset((date.Ticks / d.Ticks) * d.Ticks, date.Offset);
    }
}
