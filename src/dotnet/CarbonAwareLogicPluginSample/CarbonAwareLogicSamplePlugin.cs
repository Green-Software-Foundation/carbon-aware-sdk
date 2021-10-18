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

        public EmissionsData GetEmissionsDataForLocationByTime(Location location, DateTime time)
        {
            return _emissionsData.FirstOrDefault(ed => ed.Location.Equals(location));
        }

        public EmissionsData GetEmissionsDataForLocationByTimeWindow(Location location, TimeWindow timeWindow)
        {
            return _emissionsData.FirstOrDefault(ed => ed.Location.Equals(location));
        }

        public List<EmissionsData> GetEmissionsDataForLocationsByTime(List<Location> locationList, DateTime time)
        {
            return _emissionsData.Where(ed => locationList.Contains(ed.Location)).ToList();
        }

        public List<EmissionsData> GetEmissionsDataForLocationsByTimeWindow(List<Location> location, TimeWindow timeWindow)
        {
            return _emissionsData.Where(ed => location.Contains(ed.Location)).ToList();
        }
        
        public EmissionsData GetBestEmissionsDataForLocationsByTime(List<Location> location, DateTime time)
        {
            var locationEmissionsData = _emissionsData.Where(ed => location.Contains(ed.Location)).ToList();
            var min = locationEmissionsData.Min(ed => ed.Rating);

            return locationEmissionsData.First(ed => ed.Rating == min);
        }
        
    }
}
