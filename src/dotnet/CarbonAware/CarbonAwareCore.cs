namespace CarbonAware;

using CarbonAware.Interfaces;

/// <summary>
/// Carbon Aware SDK Core class, called via CLI, native, and web endpoints.
/// </summary>
public class CarbonAwareCore : ICarbonAwareBase
{
    private readonly ICarbonAwarePlugin _plugin;

    public CarbonAwareCore(ICarbonAwarePlugin plugin)
    {
        _plugin = plugin;

        //Console.WriteLine($"Carbon Aware Core loaded with carbon logic.");
        //Console.WriteLine($"\tName: '{plugin.Name}'");
        //Console.WriteLine($"\tAuthor: '{plugin.Author}'");
        //Console.WriteLine($"\tDescription: '{plugin.Description}'");
        //Console.WriteLine($"\tVersion: '{plugin.Version}'");
        //Console.WriteLine($"\tURL: '{plugin.URL}'");
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
