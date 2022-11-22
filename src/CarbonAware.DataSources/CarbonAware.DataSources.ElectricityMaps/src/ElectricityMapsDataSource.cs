using CarbonAware.DataSources.ElectricityMaps.Client;
using CarbonAware.DataSources.ElectricityMaps.Model;
using CarbonAware.Exceptions;
using CarbonAware.Interfaces;
using CarbonAware.Model;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CarbonAware.DataSources.ElectricityMaps;

/// <summary>
/// Represents a Electricity Maps data source.
/// </summary>
public class ElectricityMapsDataSource : IForecastDataSource, IEmissionsDataSource
{
    public string _name => "ElectricityMapsDataSource";

    public string _description => throw new NotImplementedException();

    public string _author => throw new NotImplementedException();

    public string _version => throw new NotImplementedException();

    private ILogger<ElectricityMapsDataSource> _logger { get; }

    private IElectricityMapsClient _electricityMapsClient { get; }

    private static readonly ActivitySource _activity = new ActivitySource(nameof(ElectricityMapsDataSource));

    private ILocationSource _locationSource { get; }

    /// <summary>
    /// Creates a new instance of the <see cref="ElectricityMapsDataSource"/> class.
    /// </summary>
    /// <param name="logger">The logger for the datasource</param>
    /// <param name="client">The ElectricityMaps Client</param>
    /// <param name="locationSource">The location source to be used to convert a location name to geocoordinates.</param>
    public ElectricityMapsDataSource(ILogger<ElectricityMapsDataSource> logger, IElectricityMapsClient client, ILocationSource locationSource)
    {
        this._logger = logger;
        this._electricityMapsClient = client;
        this._locationSource = locationSource;
    }

    /// <inheritdoc />
    public async Task<EmissionsForecast> GetCurrentCarbonIntensityForecastAsync(Location location)
    {
        using (var activity = _activity.StartActivity())
        {
            ForecastedCarbonIntensityData forecast;
            var geolocation = await this._locationSource.ToGeopositionLocationAsync(location);
            if (geolocation.Latitude != null && geolocation.Latitude != null)
                forecast = await this._electricityMapsClient.GetForecastedCarbonIntensityAsync (geolocation.Latitude.ToString() ?? "", geolocation.Longitude.ToString() ?? "");
            else
            {
                forecast = await this._electricityMapsClient.GetForecastedCarbonIntensityAsync (geolocation.Name ?? "");
            }

            return ToEmissionsForecast(location, forecast);
        }
    }

    private static EmissionsForecast ToEmissionsForecast(Location location, ForecastedCarbonIntensityData forecast)
    {
        var requestedAt = DateTimeOffset.UtcNow;
        var emissionsForecast = (EmissionsForecast)forecast;
        var duration = emissionsForecast.GetDurationBetweenForecastDataPoints();
        emissionsForecast.Location = location;
        emissionsForecast.RequestedAt = requestedAt;
        emissionsForecast.ForecastData = emissionsForecast.ForecastData.Select(d =>
        {
            d.Location = location.Name;
            d.Duration = duration;
            return d;
        });

        return emissionsForecast;
    }

    /// <inheritdoc />
    public async Task<EmissionsForecast> GetCarbonIntensityForecastAsync(Location location, DateTimeOffset requestedAt)
    {
        await Task.Run(() => true);
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmissionsData>> GetCarbonIntensityAsync(IEnumerable<Location> locations, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime)
    {
        this._logger.LogDebug("Getting carbon intensity for locations {locations} for period {periodStartTime} to {periodEndTime}.", locations, periodStartTime, periodEndTime);
        using (var activity = _activity.StartActivity())
        {
            List<EmissionsData> result = new();
            foreach (var location in locations)
            {
                IEnumerable<EmissionsData> interimResult = await GetCarbonIntensityAsync(location, periodStartTime, periodEndTime);
                result.AddRange(interimResult);
            }
            return result;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmissionsData>> GetCarbonIntensityAsync(Location location, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime)
    {
        using (var activity = _activity.StartActivity())
        {
            var geolocation = await this._locationSource.ToGeopositionLocationAsync(location);
            IEnumerable<CarbonIntensity> historyCarbonIntensity;
            DateTime now = DateTime.UtcNow;
            var isDateRangeWithin24Hours = (periodStartTime > now.AddHours(-24) && periodStartTime <= now) && (periodEndTime > now.AddHours(-24) && periodEndTime <= now);
            if (isDateRangeWithin24Hours)
            {
                historyCarbonIntensity = await GetRecentCarbonInstensityData(geolocation);
            }
            else
            {
                historyCarbonIntensity = await GetPastCarbonIntensityData(periodStartTime, periodEndTime, geolocation);
            }
            
            return HistoryCarbonIntensityToEmissionsData(location, historyCarbonIntensity, periodStartTime, periodEndTime);
        }
    }

    private async Task<IEnumerable<CarbonIntensity>> GetPastCarbonIntensityData(DateTimeOffset periodStartTime, DateTimeOffset periodEndTime, Location geolocation)
    {
        PastRangeData data;
        if (geolocation.Latitude != null && geolocation.Latitude != null)
            data = await this._electricityMapsClient.GetPastRangeDataAsync(geolocation.Latitude.ToString() ?? "", geolocation.Longitude.ToString() ?? "", periodStartTime, periodEndTime);
        else
        {
            data = await this._electricityMapsClient.GetPastRangeDataAsync(geolocation.Name ?? "", periodStartTime, periodEndTime);
        }

        return data.HistoryData;
    }

    private async Task<IEnumerable<CarbonIntensity>> GetRecentCarbonInstensityData(Location geolocation)
    {
        HistoryCarbonIntensityData data;
        if (geolocation.Latitude != null && geolocation.Latitude != null)
            data = await this._electricityMapsClient.GetRecentCarbonIntensityHistoryAsync(geolocation.Latitude.ToString() ?? "", geolocation.Longitude.ToString() ?? "");
        else
        {
            data = await this._electricityMapsClient.GetRecentCarbonIntensityHistoryAsync(geolocation.Name ?? "");
        }

        return data.HistoryData;
    }

    private IEnumerable<EmissionsData> HistoryCarbonIntensityToEmissionsData(Location location, IEnumerable<CarbonIntensity> data, DateTimeOffset startTime, DateTimeOffset endTime)
    {
        IEnumerable<EmissionsData> emissions;
        var duration = GetDurationFromHistoryDataPointsOrDefault(data, TimeSpan.Zero);
        emissions = data.Select(d =>
        {
            var emission = (EmissionsData) d;
            emission.Location = location.Name;
            emission.Time = d.DateTime;
            emission.Duration = duration;
            return emission;
        });

        return emissions;
    }

    private TimeSpan GetDurationFromHistoryDataPointsOrDefault(IEnumerable<CarbonIntensity> carbonIntensityDataPoints, TimeSpan defaultValue)
    {
        try
        {
            return GetDurationFromHistoryDataPoints(carbonIntensityDataPoints);
        }
        catch (CarbonAwareException)
        {
            return defaultValue;
        }
    }

    private TimeSpan GetDurationFromHistoryDataPoints(IEnumerable<CarbonIntensity> dataPoints)
    {
        var firstPoint = dataPoints.FirstOrDefault();
        var secondPoint = dataPoints.Skip(1)?.FirstOrDefault();

        var first = firstPoint ?? throw new CarbonAwareException("Too few data points returned");
        var second = secondPoint ?? throw new CarbonAwareException("Too few data points returned");

        // Handle chronological and reverse-chronological data by using `.Duration()` to get
        // the absolute value of the TimeSpan between the two points.
        return first.DateTime.Subtract(second.DateTime).Duration();
    }
}
