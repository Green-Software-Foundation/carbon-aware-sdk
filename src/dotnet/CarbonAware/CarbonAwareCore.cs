using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CarbonAware
{
    public interface ICarbonAwarePlugin : ICarbonAwareBase
    {
        string Name { get; }
        string Description { get; }
        string Author { get; }
        string Version { get; }
        object URL { get; }
    }

    public interface ICarbonAwareBase
    {
        EmissionsData GetEmissionsDataForLocationByTime(Location location, DateTime time);
        List<EmissionsData> GetEmissionsDataForLocationsByTime(List<Location> locations, DateTime time);
        EmissionsData GetEmissionsDataForLocationByTimeWindow(Location location, TimeWindow timeWindow);
        List<EmissionsData> GetEmissionsDataForLocationsByTimeWindow(List<Location> locations, TimeWindow timeWindow);

        EmissionsData GetBestEmissionsDataForLocationsByTime(List<Location> locations, DateTime time);
        // more best emissions 
    }

    public interface ICarbonDataService
    {
        List<EmissionsData> GetData();
    }

    /// <summary>
    /// Carbon Aware SDK Core Library 
    /// </summary>
    public class CarbonAwareCore : ICarbonAwareBase
    {
        private readonly ICarbonAwarePlugin _plugin;

        public CarbonAwareCore(ICarbonAwarePlugin plugin)
        {
            _plugin = plugin;

            Console.WriteLine($"Carbon Aware Core loaded with carbon logic.");
            Console.WriteLine($"\tName: '{plugin.Name}'");
            Console.WriteLine($"\tAuthor: '{plugin.Author}'");
            Console.WriteLine($"\tDescription: '{plugin.Description}'");
            Console.WriteLine($"\tVersion: '{plugin.Version}'");
            Console.WriteLine($"\tURL: '{plugin.URL}'");
        }

        /// <summary>
        /// Get emissions data for a specific locations, at a specific time
        /// </summary>
        /// <param name="location">The locations for which the emissions data should be retrieved.</param>
        /// <param name="time">The date and time for which the emissions data should be retrieved.</param>
        /// <returns>magic</returns>
        public EmissionsData GetEmissionsDataForLocationByTime(Location location, DateTime time)
        {
            // telemetry here 
            return _plugin.GetEmissionsDataForLocationByTime(location, time);
        }

        public List<EmissionsData> GetEmissionsDataForLocationsByTime(List<Location> locations, DateTime time)
        {
            return _plugin.GetEmissionsDataForLocationsByTime(locations, time);
        }

        public EmissionsData GetEmissionsDataForLocationByTimeWindow(Location location, TimeWindow timeWindow)
        {
            throw new NotImplementedException();
        }

        public List<EmissionsData> GetEmissionsDataForLocationsByTimeWindow(List<Location> locations, TimeWindow timeWindow)
        {
            throw new NotImplementedException();
        }

        public EmissionsData GetBestEmissionsDataForLocationsByTime(List<Location> locations, DateTime time)
        {
            return _plugin.GetBestEmissionsDataForLocationsByTime(locations, time);
        }
        
    }
}
