using CarbonAware.DataSources.ElectricityMaps.Client;
using CarbonAware.DataSources.ElectricityMaps.Model;
using CarbonAware.Interfaces;
using CarbonAware.Model;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CarbonAware.DataSources.ElectricityMaps;

/// <summary>
/// Represents a Electricity Maps data source.
/// </summary>
public class ElectricityMapsDataSource : IForecastDataSource
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
}
