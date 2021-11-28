using CarbonAware.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CarbonAware
{
 
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
                
        public List<EmissionsData> GetEmissionsDataForLocationByTime(string location, DateTime time, DateTime? toTime = null, TimeSpan? duration = null)
        {
            return _plugin.GetEmissionsDataForLocationByTime(location, time, toTime, duration);
        }

        public List<EmissionsData> GetEmissionsDataForLocationsByTime(List<string> locations, DateTime time, DateTime? toTime = null, TimeSpan? duration = null)
        {
            return _plugin.GetEmissionsDataForLocationsByTime(locations, time, toTime, duration);
        }

        public List<EmissionsData> GetBestEmissionsDataForLocationsByTime(List<string> locations, DateTime time, DateTime? toTime = null, TimeSpan? duration = null)
        {
            return _plugin.GetBestEmissionsDataForLocationsByTime(locations, time, toTime);
        }
    }
}
