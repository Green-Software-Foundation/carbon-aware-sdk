using CarbonAware;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CarbonAwareLogicPluginSample
{
    public class CarbonAwareLogicPlugin: ICarbonAwarePlugin
    {
        public string Name => "CarbonAwareLogicSamplePlugin";

        public string Description => "Example plugin for Carbon Aware SDK";

        public string Author => "Green Software Foundation";

        public string Version => "0.0.1";

        public object URL => "http://github.com/greensoftwarefoudation";

        private ICarbonDataService _carbonDataService { get; }
        private List<EmissionsData> _emissionsData { get; }
        public CarbonAwareLogicPlugin(ICarbonDataService carbonDataServiceService)
        {
            this._carbonDataService = carbonDataServiceService;
            this._emissionsData = this._carbonDataService.GetData();
        }

        public EmissionsData GetEmissionsDataForLocationByTime(string location, DateTime time)
        {
            var edList = _emissionsData.Where(ed => ed.Location.Equals(location)).ToList();

            var beforeNow = edList.Where(ed => ed.Time <= time).ToList();

            if (beforeNow.Count == 0)
            {
                return EmissionsData.None;
            }

            var maxTime = beforeNow.Max(ed => ed.Time);

            return beforeNow.FirstOrDefault(ed => ed.Time == maxTime);
        }

        public EmissionsData GetEmissionsDataForLocationByTimeWindow(string location, TimeWindow timeWindow)
        {
            return _emissionsData.FirstOrDefault(ed => ed.Location.Equals(location));
        }

        public List<EmissionsData> GetEmissionsDataForLocationsByTime(List<string> locations, DateTime time)
        {
            return _emissionsData.Where(ed => locations.Contains(ed.Location)).ToList();
        }

        public List<EmissionsData> GetEmissionsDataForLocationsByTimeWindow(List<string> locations, TimeWindow timeWindow)
        {
            return _emissionsData.Where(ed => locations.Contains(ed.Location)).ToList();
        }
        
        public EmissionsData GetBestEmissionsDataForLocationsByTime(List<string> locations, DateTime time)
        {
            var locationEmissionsData = _emissionsData.Where(ed => locations.Contains(ed.Location)).ToList();
            var min = locationEmissionsData.Min(ed => ed.Rating);

            return locationEmissionsData.First(ed => ed.Rating == min);
        }
        
    }
}
