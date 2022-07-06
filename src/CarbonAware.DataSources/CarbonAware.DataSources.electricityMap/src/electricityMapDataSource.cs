using CarbonAware.Exceptions;
using CarbonAware.Interfaces;
using CarbonAware.Model;
using CarbonAware.Tools.electricityMapClient;
using CarbonAware.Tools.electricityMapClient.Model;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CarbonAware.DataSources.electricityMap;

/// <summary>
/// Reprsents a electricityMap data source.
/// </summary>
public class electricityMapDataSource : ICarbonIntensityDataSource
{
    public string Name => "electricityMapDataSource";

    public string Description => throw new NotImplementedException();

    public string Author => throw new NotImplementedException();

    public string Version => throw new NotImplementedException();

    private ILogger<electricityMapDataSource> Logger { get; }

    private IelectricityMapClient electricityMapClient { get; }

    private ActivitySource ActivitySource { get; }

    private ILocationSource LocationSource { get; }

    double ICarbonIntensityDataSource.MinSamplingWindow => throw new NotImplementedException();

    /// <summary>
    /// Creates a new instance of the <see cref=electricityMapDataSource"/> class.
    /// </summary>
    /// <param name="logger">The logger for the datasource</param>
    /// <param name="client">The electricityMap Client</param>
    /// <param name="activitySource">The activity source for telemetry.</param>
    /// <param name="locationSource">The location source to be used to convert a location to BA's.</param>
    public electricityMapDataSource(ILogger<electricityMapDataSource> logger, IelectricityMapClient client, ActivitySource activitySource, ILocationSource locationSource)
    {
        this.Logger = logger;
        this.electricityMapClient = client;
        this.ActivitySource = activitySource;
        this.LocationSource = locationSource;
    }

    /// <inheritdoc />
    /// TODO: For electricityMap no need Datetime to get latest Carbon Intensity
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

    // TODO: Need implemention such as Converter from Location to Zone like Watttime
    private Task<IEnumerable<EmissionsData>> GetCarbonIntensityAsync(Location location, DateTimeOffset? periodStartTime, DateTimeOffset? periodEndTime) => throw new NotImplementedException();


    Task<EmissionsForecast> ICarbonIntensityDataSource.GetCurrentCarbonIntensityForecastAsync(Location location)
    {
        throw new NotImplementedException();
    }
}
