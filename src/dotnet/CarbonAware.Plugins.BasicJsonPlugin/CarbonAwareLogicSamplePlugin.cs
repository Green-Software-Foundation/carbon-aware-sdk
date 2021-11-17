using CarbonAware;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CarbonAware.Plugins.BasicJsonPlugin
{
    public class CarbonAwareBasicDataPlugin : ICarbonAwarePlugin
    {
        public string Name => "CarbonAwareLogicSamplePlugin";

        public string Description => "Example plugin for Carbon Aware SDK";

        public string Author => "Green Software Foundation";

        public string Version => "0.0.1";

        public object URL => "http://github.com/greensoftwarefoudation";

        private ICarbonAwareStaticDataService CarbonAwareStaticDataService { get; }
        private List<EmissionsData> _emissionsData { get; }
        public CarbonAwareBasicDataPlugin(ICarbonAwareStaticDataService carbonAwareStaticDataService)
        {
            this.CarbonAwareStaticDataService = carbonAwareStaticDataService;
            this._emissionsData = this.CarbonAwareStaticDataService.GetData();
        }

        /// <summary>
        /// Returns the most recent prior emissions data record for the specified location.
        /// </summary>
        /// <param name="location">The name of the location to filter by.</param>
        /// <param name="time">The time to retrieve the most recent data for.</param>
        /// <returns>A single emissions data record for the location, and EmissionsData.None if no results are found.</returns>
        public List<EmissionsData> GetEmissionsDataForLocationByTime(string location, DateTime time, DateTime ? toTime = null)
        {
            List<EmissionsData> matchingEmissionsData = new List<EmissionsData>();

            List<EmissionsData> emissionsDataForLocation = _emissionsData.Where(ed => ed.Location.Equals(location)).ToList();
            
            List<EmissionsData> emissionsDataBeforeTime = emissionsDataForLocation.Where(ed => ed.Time <= time).ToList();

            if (emissionsDataBeforeTime.Count != 0)
            {
                var mostRecentEmissionsAtTime = emissionsDataBeforeTime.MaxBy(ed => ed.Time);
                matchingEmissionsData.Add(mostRecentEmissionsAtTime);
            }

            if (toTime != null)
            {
                var timeWindowEmissions = emissionsDataForLocation.Where(ed => ed.Time > time && ed.Time <= toTime).ToList();
                matchingEmissionsData.AddRange(timeWindowEmissions);
            }

            return matchingEmissionsData;
        }

        /// <summary>
        /// Returns the most recent prior emissions data record for a list of specified locations.
        /// </summary>
        /// <param name="locations">The name of the locations to filter by.</param>
        /// <param name="time">The time to retrieve the most recent data for.</param>
        /// <returns>A List&lt;EmissionsData&gt; for each emissions data record for each location available.  If no records are found, returns an empty List.</returns>
        public List<EmissionsData> GetEmissionsDataForLocationsByTime(List<string> locations, DateTime time, DateTime? toTime = null)
        {
            var results = new List<EmissionsData>();
            foreach(var location in locations)
            {
                var emissionsData = GetEmissionsDataForLocationByTime(location, time, toTime);
                results.AddRange(emissionsData);
            }
            return results;
        }

        /// <summary>
        /// Returns the lowest emissions record for a list of specified locations at a specific time.
        /// </summary>
        /// <param name="location">The name of the locations to filter by.</param>
        /// <param name="time">The time to retrieve the most recent data for.</param>
        /// <returns>>A single emissions data record for the location based on the "best" emissions i.e. in thie case, the lowest.  Returns EmissionsData.None if no results are found.</returns>
        public EmissionsData GetBestEmissionsDataForLocationsByTime(List<string> locations, DateTime time, DateTime? toTime = null)
        {
            var emissions = GetEmissionsDataForLocationsByTime(locations, time, toTime);
            if(emissions.Count == 0)
            {
                return EmissionsData.None;
            }
            return emissions.MinBy(ed => ed.Rating);
        }
    }
}
