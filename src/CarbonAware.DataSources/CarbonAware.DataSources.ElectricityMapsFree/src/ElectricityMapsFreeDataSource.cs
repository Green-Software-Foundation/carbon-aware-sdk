using CarbonAware.DataSources.ElectricityMapsFree.Client;
using CarbonAware.DataSources.ElectricityMapsFree.Model;
using CarbonAware.Interfaces;
using CarbonAware.Model;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CarbonAware.DataSources.ElectricityMapsFree;
/// <summary>
/// Represents a Electricity Maps Free data source.
/// </summary>
public class ElectricityMapsFreeDataSource : IForecastDataSource//, IEmissionsDataSource
{
    public string _name => "ElectricityMapsFreeDataSource";

    public string _description => throw new NotImplementedException();

    public string _author => throw new NotImplementedException();

    public string _version => throw new NotImplementedException();

    private ILogger<ElectricityMapsFreeDataSource> _logger { get; }

    private IElectricityMapsFreeClient _electricityMapsFreeClient { get; }

    private static readonly ActivitySource Activity = new ActivitySource(nameof(ElectricityMapsFreeDataSource));

    private ILocationSource _locationSource { get; }

    /// <summary>
    /// Creates a new instance of the <see cref="ElectricityMapsFreeDataSource"/> class.
    /// </summary>
    /// <param name="logger">The logger for the datasource</param>
    /// <param name="client">The ElectricityMapsFree Client</param>
    /// <param name="locationSource">The location source to be used to convert a location name to geocoordinates.</param>
    public ElectricityMapsFreeDataSource(ILogger<ElectricityMapsFreeDataSource> logger, IElectricityMapsFreeClient client, ILocationSource locationSource)
    {
        this._logger = logger;
        this._electricityMapsFreeClient = client;
        this._locationSource = locationSource;
    }
    /// <inheritdoc />
    public async Task<EmissionsForecast> GetCurrentCarbonIntensityForecastAsync(Location location)
    {
        this._logger.LogInformation($"Getting carbon intensity forecast for location {location}");

        using (var activity = Activity.StartActivity())
        {
            var geolocation = await this._locationSource.ToGeopositionLocationAsync(location);
            Forecast? data;
            if (geolocation.Latitude != null && geolocation.Latitude != null)
                data = await this._electricityMapsFreeClient.GetCurrentForecastAsync(geolocation.LatitudeAsCultureInvariantString(), geolocation.LongitudeAsCultureInvariantString());
            else
                data = await this._electricityMapsFreeClient.GetCurrentForecastAsync(geolocation.Name ?? "");

            // Link statement to convert Electricity Map forecast data into EmissionsData for the CarbonAware SDK.
            var forecastData = data.ForecastData.Select(e => new EmissionsData()
            {
                Location = e.CountryCodeAbbreviation,
                Rating = e.Data.CarbonIntensity,
                Time = e.Data.Datetime,
                Duration = new TimeSpan(2, 0, 0), // TODO: it seems that they update the forecast every 2 hours (or 1??), therefore we use this. But it's just a hypothesis!
            });

            return new EmissionsForecast()
            {
                /*

                /// <summary>
                /// Gets or sets the start time of the forecast data points.
                /// </summary>
                public DateTimeOffset DataStartAt { get; set; }

                /// <summary>
                /// Gets or sets the end time of the forecast data points.
                /// </summary>
                public DateTimeOffset DataEndAt { get; set; }

                /// <summary>
                /// Gets or sets rolling average window duration.
                /// </summary>
                public TimeSpan WindowSize { get; set; }

                /// <summary>
                /// Gets or sets the forecast data points.
                /// </summary>
                public IEnumerable<EmissionsData> ForecastData { get; set; } = new List<EmissionsData>();

                /// <summary>
                /// Gets or sets the optimal data points within the ForecastData set.
                /// </summary>
                public IEnumerable<EmissionsData> OptimalDataPoints { get; set; } = new List<EmissionsData>();
    
                */
                RequestedAt = DateTimeOffset.UtcNow,
                GeneratedAt = data.GeneratedAt,
                Location = location,
                DataStartAt = data.GeneratedAt,
                //DataEndAt = 

                ForecastData = forecastData,
            };
        }
    }

    /// <inheritdoc />
    public async Task<EmissionsForecast> GetCarbonIntensityForecastAsync(Location location, DateTimeOffset requestedAt)
        => throw new NotImplementedException("The API of CO2 Signal does not provide this service");


    /*
    /// <inheritdoc />
    public async Task<IEnumerable<EmissionsData>> GetCarbonIntensityAsync(IEnumerable<Location> locations, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime)
    {
        this._logger.LogDebug("Getting carbon intensity for locations {locations} for period {periodStartTime} to {periodEndTime}.", locations, periodStartTime, periodEndTime);
        this._logger.LogDebug("Not implemented yet, throwing");
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmissionsData>> GetCarbonIntensityAsync(Location location, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime)
    {
    }
    */
}
