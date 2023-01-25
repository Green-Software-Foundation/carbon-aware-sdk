using CarbonAware.DataSources.WattTime.Client;
using CarbonAware.DataSources.WattTime.Model;
using CarbonAware.Interfaces;
using CarbonAware.LocationSources.Exceptions;
using CarbonAware.Model;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Globalization;

namespace CarbonAware.DataSources.WattTime;

/// <summary>
/// Represents a WattTime data source.
/// </summary>
public class WattTimeDataSource : IEmissionsDataSource, IForecastDataSource
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
    /// <param name="locationSource">The location source to be used to convert a location to BA's.</param>
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
        List<EmissionsData> result = new ();
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
        var balancingAuthority = await this.GetBalancingAuthority(location);
        var (newStartTime, newEndTime) = IntervalHelper.ExtendTimeByWindow(periodStartTime, periodEndTime, MinSamplingWindow);
        var data = await this.WattTimeClient.GetDataAsync(balancingAuthority, newStartTime, newEndTime);
        if (Logger.IsEnabled(LogLevel.Debug))
        {
            Logger.LogDebug($"Found {data.Count()} total forecasts for location {location} for period {periodStartTime} to {periodEndTime}.");
        }
        var windowData = ConvertToEmissionsData(data);
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
        var balancingAuthority = await this.GetBalancingAuthority(location);
        var forecast = await this.WattTimeClient.GetCurrentForecastAsync(balancingAuthority); 
        return ForecastToEmissionsForecast(forecast, location, DateTimeOffset.UtcNow);
    }

    /// <inheritdoc />
    public async Task<EmissionsForecast> GetCarbonIntensityForecastAsync(Location location, DateTimeOffset requestedAt)
    {
        this.Logger.LogInformation($"Getting carbon intensity forecast for location {location} requested at {requestedAt}");
        var balancingAuthority = await this.GetBalancingAuthority(location);
        var roundedRequestedAt = TimeToLowestIncrement(requestedAt);
        var forecast = await this.WattTimeClient.GetForecastOnDateAsync(balancingAuthority, roundedRequestedAt);
        if (forecast == null)
        {
            var ex = new ArgumentException($"No forecast was generated at the requested time {roundedRequestedAt}");
            Logger.LogError(ex, ex.Message);
            throw ex;
        }
        // keep input from the user.
        return ForecastToEmissionsForecast(forecast, location, requestedAt); 
    }

    private EmissionsForecast ForecastToEmissionsForecast(Forecast forecast, Location location, DateTimeOffset requestedAt) 
    {
        var duration = GetDurationFromGridEmissionDataPoints(forecast.ForecastData);
        var forecastData = forecast.ForecastData.Select(e => new EmissionsData()
        {
            Location = e.BalancingAuthorityAbbreviation,
            Rating = ConvertMoerToGramsPerKilowattHour(e.Value),
            Time = e.PointTime,
            Duration = duration
        });
        var emissionsForecast = new EmissionsForecast()
        {
            GeneratedAt = forecast.GeneratedAt,
            Location = location,
            ForecastData = forecastData
        };
        emissionsForecast.RequestedAt = requestedAt;
        return emissionsForecast;
    }

    internal double ConvertMoerToGramsPerKilowattHour(double value)
    {
        return value * LBS_TO_GRAMS_CONVERSION_FACTOR / MWH_TO_KWH_CONVERSION_FACTOR;
    }

    private IEnumerable<EmissionsData> ConvertToEmissionsData(IEnumerable<GridEmissionDataPoint> gridEmissionDataPoints)
    {
        var defaultDuration = GetDurationFromGridEmissionDataPointsOrDefault(gridEmissionDataPoints, TimeSpan.Zero);
        
        // Linq statement to convert WattTime forecast data into EmissionsData for the CarbonAware SDK.
        return gridEmissionDataPoints.Select(e => new EmissionsData() 
                    { 
                        Location = e.BalancingAuthorityAbbreviation, 
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

    private async Task<BalancingAuthority> GetBalancingAuthority(Location location)
    {
        BalancingAuthority balancingAuthority;
        try
        {
            var geolocation = await this.LocationSource.ToGeopositionLocationAsync(location);
            balancingAuthority = await WattTimeClient.GetBalancingAuthorityAsync(geolocation.LatitudeAsCultureInvariantString(), geolocation.LongitudeAsCultureInvariantString());
        }
        catch(Exception ex) when (ex is LocationConversionException ||  ex is WattTimeClientHttpException)
        {
            Logger.LogError(ex, "Failed to convert the location {location} into a Balancing Authority.", location);
            throw;
        }

        Logger.LogDebug("Converted location {location} to balancing authority {balancingAuthorityAbbreviation}", location, balancingAuthority.Abbreviation);

        return balancingAuthority;
    }

    private DateTimeOffset TimeToLowestIncrement(DateTimeOffset date, int minutes = 5)
    {
        var d = TimeSpan.FromMinutes(minutes);
        return new DateTimeOffset((date.Ticks / d.Ticks) * d.Ticks, date.Offset);
    }
}
