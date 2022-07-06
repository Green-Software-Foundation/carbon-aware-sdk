using CarbonAware.Exceptions;
using CarbonAware.Interfaces;
using CarbonAware.Model;
using CarbonAware.Tools.electricityMapClient;
using CarbonAware.Tools.electricityMapClient.Model;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CarbonAware.DataSources.electricityMap;

/// <summary>
/// Reprsents a wattime data source.
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

    private async Task<IEnumerable<EmissionsData>> GetCarbonIntensityAsync(Location location, DateTimeOffset? periodStartTime, DateTimeOffset? periodEndTime)
    {
        this.Logger.LogInformation("Getting carbon intensity for location {location} for period {periodStartTime} to {periodEndTime}.", location, periodStartTime, periodEndTime);

        using (var activity = ActivitySource.StartActivity())
        {
            Zone zone;
            activity?.AddTag("location", location);
            // TODO: Need Converter from Location to Zone
            zone = location

            // TODO: For personal, only GetCurrentForecastAsync is available
            var data = (await this.electricityMapClient.GetCurrentForecastAsync(zone)).ToList();

            Logger.LogDebug("Found {count} total forecasts for location {location} for period {periodStartTime} to {periodEndTime}.", data.Count, location, periodStartTime, periodEndTime);

            // Linq statement to convert WattTime forecast data into EmissionsData for the CarbonAware SDK.
            // TODO: Need to get data property from nested json structure for electricityMap
            var result = data.Select(e => new EmissionsData() 
            { 
                Location = e.countryCodeAbbreviation, 
                Rating = e.data, 
                Time = e.data 
            });

            if (Logger.IsEnabled(LogLevel.Debug))
            {
                Logger.LogDebug("Found {count} total emissions data records for location {location} for period {periodStartTime} to {periodEndTime}.", result.ToList().Count, location, periodStartTime, periodEndTime);
            }

            return result;
        }
    }

}
