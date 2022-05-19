namespace CarbonAware;

using CarbonAware.Interfaces;
using Microsoft.Extensions.Logging;

/// <summary>
/// Carbon Aware SDK Core class, called via CLI, native, and web endpoints.
/// </summary>
public class CarbonAwareCore : ICarbonAwareBase
{
    private readonly ICarbonAwarePlugin _plugin;
    private readonly ILogger<CarbonAwareCore> _logger;


    public CarbonAwareCore(ILogger<CarbonAwareCore> logger, ICarbonAwarePlugin plugin)
    {
        this._plugin = plugin;
        this._logger = logger;

        _logger.LogInformation("Carbon Aware Core loaded with carbon logic.");

        _logger.LogDebug($"\tName: '{plugin.Name}'");
        _logger.LogDebug($"\tAuthor: '{plugin.Author}'");
        _logger.LogDebug($"\tDescription: '{plugin.Description}'");
        _logger.LogDebug($"\tVersion: '{plugin.Version}'");
        _logger.LogDebug($"\tURL: '{plugin.URL}'");
    }

    public List<EmissionsData> GetEmissionsDataForLocationByTime(string location, DateTime time, DateTime? toTime = null, int durationMinutes = 0)
    {
        return _plugin.GetEmissionsDataForLocationByTime(location, time, toTime, durationMinutes);
    }

    public List<EmissionsData> GetEmissionsDataForLocationsByTime(List<string> locations, DateTime time, DateTime? toTime = null, int durationMinutes = 0)
    {
        return _plugin.GetEmissionsDataForLocationsByTime(locations, time, toTime, durationMinutes);
    }

    public List<EmissionsData> GetBestEmissionsDataForLocationsByTime(List<string> locations, DateTime time, DateTime? toTime = null, int durationMinutes = 0)
    {
        return _plugin.GetBestEmissionsDataForLocationsByTime(locations, time, toTime, durationMinutes);
    }
}
