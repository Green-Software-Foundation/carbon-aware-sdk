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
                
        public EmissionsData GetEmissionsDataForLocationByTime(string location, DateTime time)
        {
            return _plugin.GetEmissionsDataForLocationByTime(location, time);
        }

        public List<EmissionsData> GetEmissionsDataForLocationsByTime(List<string> locations, DateTime time)
        {
            return _plugin.GetEmissionsDataForLocationsByTime(locations, time);
        }

        public EmissionsData GetEmissionsDataForLocationByTimeWindow(string location, TimeWindow timeWindow)
        {
            throw new NotImplementedException();
        }

        public List<EmissionsData> GetEmissionsDataForLocationsByTimeWindow(List<string> locations, TimeWindow timeWindow)
        {
            throw new NotImplementedException();
        }

        public EmissionsData GetBestEmissionsDataForLocationsByTime(List<string> locations, DateTime time)
        {
            return _plugin.GetBestEmissionsDataForLocationsByTime(locations, time);
        }
        
    }
}
