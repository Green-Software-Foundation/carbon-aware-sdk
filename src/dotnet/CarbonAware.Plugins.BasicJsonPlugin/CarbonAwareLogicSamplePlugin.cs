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
        public EmissionsData GetEmissionsDataForLocationByTime(string location, DateTime time)
        {
            var emissionsBeforeNowAtLocation = _emissionsData.Where(ed => ed.Location.Equals(location) && ed.Time <= time).ToList();

            if (emissionsBeforeNowAtLocation.Count == 0)
            {
                return EmissionsData.None;
            }

            var mostRecentEmissions = emissionsBeforeNowAtLocation.MaxBy(ed => ed.Time);

            return mostRecentEmissions;
        }

        

        /// <summary>
        /// Returns the most recent prior emissions data record for a list of specified locations.
        /// </summary>
        /// <param name="location">The name of the locations to filter by.</param>
        /// <param name="time">The time to retrieve the most recent data for.</param>
        /// <returns>A List&lt;EmissionsData&gt; for each emissions data record for each location available.  If no records are found, returns an empty List.</returns>
        public List<EmissionsData> GetEmissionsDataForLocationsByTime(List<string> locations, DateTime time)
        {
            var results = new List<EmissionsData>();
            foreach(var location in locations)
            {
                var emissionsData = GetEmissionsDataForLocationByTime(location, time);
                if(emissionsData != EmissionsData.None)
                {
                    results.Add(emissionsData);
                }
            }
            return results;
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <param name="locations"></param>
        /// <param name="timeWindow"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public List<EmissionsData> GetEmissionsDataForLocationsByTimeWindow(List<string> locations, TimeWindow timeWindow)
        {
            throw new NotImplementedException();
            return _emissionsData.Where(ed => locations.Contains(ed.Location)).ToList();
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <param name="location"></param>
        /// <param name="timeWindow"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public EmissionsData GetEmissionsDataForLocationByTimeWindow(string location, TimeWindow timeWindow)
        {
            throw new NotImplementedException();
            return _emissionsData.FirstOrDefault(ed => ed.Location.Equals(location));
        }

        /// <summary>
        /// Returns the lowest emissions record for a list of specified locations at a specific time.
        /// </summary>
        /// <param name="location">The name of the locations to filter by.</param>
        /// <param name="time">The time to retrieve the most recent data for.</param>
        /// <returns>>A single emissions data record for the location based on the "best" emissions i.e. in thie case, the lowest.  Returns EmissionsData.None if no results are found.</returns>
        public EmissionsData GetBestEmissionsDataForLocationsByTime(List<string> locations, DateTime time)
        {
            var emissions = GetEmissionsDataForLocationsByTime(locations, time);
            if(emissions.Count == 0)
            {
                return EmissionsData.None;
            }
            return emissions.MinBy(ed => ed.Rating);
        }
    }
}
