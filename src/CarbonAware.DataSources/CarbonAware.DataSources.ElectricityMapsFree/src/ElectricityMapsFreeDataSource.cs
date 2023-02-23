using CarbonAware.DataSources.ElectricityMapsFree.Client;
using CarbonAware.DataSources.ElectricityMapsFree.Model;
using CarbonAware.Interfaces;
using CarbonAware.Model;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CarbonAware.DataSources.ElectricityMapsFree;
/// <summary>
/// Represents an Electricity Maps Free data source.
/// </summary>
public class ElectricityMapsFreeDataSource : IEmissionsDataSource
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
    public async Task<IEnumerable<EmissionsData>> GetCarbonIntensityAsync(IEnumerable<Location> locations, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime)
    {
        List<EmissionsData> EmissionsDataList = new List<EmissionsData>();

        foreach (var location in locations)
        {
            var emissionsDataForLocation = await GetCarbonIntensityAsync(location, periodStartTime, periodEndTime);
            EmissionsDataList.AddRange(emissionsDataForLocation);
        }
        
        return EmissionsDataList;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmissionsData>> GetCarbonIntensityAsync(Location location, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime)
    {
        this._logger.LogInformation($"Getting current carbon intensity emissions for location {location}");

        Location? geolocation = null;
        bool coordinatesAvailable = false;
        try
        {
            geolocation = await this._locationSource.ToGeopositionLocationAsync(location);
            if (geolocation.Latitude != null && geolocation.Longitude != null)
            {
                coordinatesAvailable = true;
            }
        }
        catch (ArgumentException)
        {
            
        }

        GridEmissionDataPoint gridEmissionData;
        if (coordinatesAvailable && geolocation != null)
        {
            gridEmissionData = await this._electricityMapsFreeClient.GetCurrentEmissionsAsync(geolocation.LatitudeAsCultureInvariantString(), geolocation.LongitudeAsCultureInvariantString());
        }
        else
        {
            gridEmissionData = await this._electricityMapsFreeClient.GetCurrentEmissionsAsync(location.Name ?? "");
        }
        
        List<EmissionsData> EmissionsDataList = new List<EmissionsData>();

        var emissionData = new EmissionsData()
        {
            Location = location.Name ?? "",
            Time = gridEmissionData.Data.Datetime,
            Rating = gridEmissionData.Data.CarbonIntensity,
            Duration = new TimeSpan(0, 0, 0)
        };
        EmissionsDataList.Add(emissionData);

        return EmissionsDataList;
    }
}
