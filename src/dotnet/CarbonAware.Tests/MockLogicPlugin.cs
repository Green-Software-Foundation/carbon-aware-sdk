using CarbonAware.Data;
using Microsoft.Extensions.Configuration;

namespace CarbonAware.Tests
{
    internal class MockLogicPlugin : ICarbonAwarePlugin
    {
        public string Name { get; set; } = "Mock Logic Plugin";
        public string Description { get; set; } = "Mock Logic Plugin for testing";
        public string Author { get; set; } = "Green Software Foundation";
        public string Version { get; set; } = "0.1";
        public object URL { get; set; } = "http://github.com/green-software-foundation";

        private ICarbonAwareStaticDataService _dataService;
        public MockLogicPlugin(ICarbonAwareStaticDataService dataService)
        {
            _dataService = dataService;
        }

        public void Configure(IConfigurationSection config)
        {

        }

        public List<EmissionsData> GetBestEmissionsDataForLocationsByTime(List<string> locations, DateTime time, DateTime? toTime = null, TimeSpan? duration = null)
        {
            return _dataService.GetData();
        }

        public List<EmissionsData> GetEmissionsDataForLocationByTime(string location, DateTime time, DateTime? toTime = null, TimeSpan? duration = null)
        {
            return _dataService.GetData();
        }

        public List<EmissionsData> GetEmissionsDataForLocationsByTime(List<string> locations, DateTime time, DateTime? toTime = null, TimeSpan? duration = null)
        {
            return _dataService.GetData();
        }
    }
}
