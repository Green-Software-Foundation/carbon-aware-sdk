using CarbonAware.Exceptions;
using CarbonAware.Interfaces;
using CarbonAware.Model;
using CarbonAware.Tools.ElectricityMapClient;
using CarbonAware.Tools.ElectricityMapClient.Model;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CarbonAware.DataSources.ElectricityMap;

/// <summary>
/// Reprsents a ElectricityMap data source.
/// </summary>
public class ElectricityMapDataSource : ICarbonIntensityDataSource
{
    public string Name => "ElectricityMapDataSource";

    public string Description => throw new NotImplementedException();

    public string Author => throw new NotImplementedException();

    public string Version => throw new NotImplementedException();

    private ILogger<ElectricityMapDataSource> Logger { get; }

    private IElectricityMapClient ElectricityMapClient { get; }

    private static readonly ActivitySource Activity = new ActivitySource(nameof(ElectricityMapDataSource));

    private ILocationSource LocationSource { get; }

    double ICarbonIntensityDataSource.MinSamplingWindow => throw new NotImplementedException();

    /// <summary>
    /// Creates a new instance of the <see cref=ElectricityMapDataSource"/> class.
    /// </summary>
    /// <param name="logger">The logger for the datasource</param>
    /// <param name="client">The ElectricityMap Client</param>
    /// <param name="activitySource">The activity source for telemetry.</param>
    /// <param name="locationSource">The location source to be used to convert a location to BA's.</param>
    public ElectricityMapDataSource(ILogger<ElectricityMapDataSource> logger, IElectricityMapClient client, ILocationSource locationSource)
    {
        this.Logger = logger;
        this.ElectricityMapClient = client;
        this.LocationSource = locationSource;
    }

    /// <inheritdoc />
    /// TODO: For traial version of ElectricityMap historical datas are not avaialbe
    //public async Task<IEnumerable<EmissionsData>> GetCarbonIntensityAsync(IEnumerable<Location> locations, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime)
    //{
    //    this.Logger.LogInformation("Getting carbon intensity for locations {locations} for period {periodStartTime} to {periodEndTime}.", locations, periodStartTime, periodEndTime);
    //    List<EmissionsData> result = new ();
    //    foreach (var location in locations)
    //    {
    //        IEnumerable<EmissionsData> interimResult = await GetCarbonIntensityAsync(location, periodStartTime, periodEndTime);
    //        result.AddRange(interimResult);
    //    }
    //    return result;
    //}

    // TODO: Need implemention for Electricity Map commercial version
    public Task<IEnumerable<EmissionsData>> GetCarbonIntensityAsync(IEnumerable<Location> locations, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime) => throw new NotImplementedException();

    /// <inheritdoc />
    public async Task<EmissionsForecast> GetCurrentCarbonIntensityForecastAsync(Location location)
    {
        this.Logger.LogInformation($"Getting carbon intensity forecast for location {location}");

        using(var activity = Activity.StartActivity())
        {
            // TODO: Watttime api use mapper from cloud region to watttime region (Balanceing Authority)
            // BalancingAuthority balancingAuthority = await this.GetBalancingAuthority(location, activity);

            // No mapper for electricity map yet, so use region data directly
            var data = await this.ElectricityMapClient.GetCurrentForecastAsync(location.RegionName);

            // Link statement to convert Electricity Map forecast data into EmissionsData for the CarbonAware SDK.
            var forecastData = data.ForecastData.Select(e => new EmissionsData()
            {
                Location = e.CountryCodeAbbreviation,
                Rating = e.Data.CarbonIntensity,
                Time = e.Data.Datetime
            });

            return new EmissionsForecast()
            {
                GeneratedAt = data.GeneratedAt,
                Location = location,
                ForecastData = forecastData,
            };
        }
    }
}
