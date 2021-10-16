using CarbonAware;
using System;
using System.Collections.Generic;

namespace CarbonAwareLogicPluginSample
{
    public class CarbonAwareLogicSamplePlugin : ICarbonAwarePlugin
    {
        public string Name => "CarbonAwareLogicSamplePlugin";

        public string Description => "Example plugin for Carbon Aware SDK";

        public string Author => "Green Software Foundation";

        public string Version => "0.0.1";

        public object URL => "http://github.com/greensoftwarefoudation";

        private static EmissionsData DummyEmissionsData()
        {
            var rating = ((float)DateTime.Now.Second) / 60f;

            return new EmissionsData()
            {
                Location = null,
                Rating = rating,
                Time = DateTime.Now
            };
        }
        public EmissionsData GetEmissionsDataForLocationByTime(Location location, DateTime time)
        {
            return DummyEmissionsData();
        }

        public EmissionsData GetEmissionsDataForLocationByTimeWindow(Location location, TimeWindow timeWindow)
        {
            return DummyEmissionsData();
        }

        public List<EmissionsData> GetEmissionsDataForLocationsByTime(List<Location> location, DateTime time)
        {
            var emissionsList = new List<EmissionsData>();
            emissionsList.Add(DummyEmissionsData());
            return emissionsList;
        }

        public List<EmissionsData> GetEmissionsDataForLocationsByTimeWindow(List<Location> location, TimeWindow timeWindow)
        {
            var emissionsList = new List<EmissionsData>();
            emissionsList.Add(DummyEmissionsData());
            return emissionsList;
        }
    }
}
