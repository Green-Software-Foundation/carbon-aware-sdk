using CarbonAware.Model;
using Microsoft.Extensions.Configuration;

namespace CarbonAware.Tests;

internal class MockLogicPlugin : ICarbonAwarePlugin
{
    public string Name { get;  } = "Mock Logic Plugin";
    public string Description { get;  } = "Mock Logic Plugin for testing";
    public string Author { get;  } = "Green Software Foundation";
    public string Version { get;  } = "0.1";
    public object URL { get;  } = "http://github.com/green-software-foundation";

    private ICarbonAwareStaticDataService _dataService;
    public MockLogicPlugin(ICarbonAwareStaticDataService dataService)
    {
        _dataService = dataService;
    }


    public void Configure(IConfigurationSection config)
    {
        
    }

    public List<EmissionsData> GetBestEmissionsDataForLocationsByTime(List<string> locations, DateTime time, DateTime? toTime = null, int durationMinutes = 0)
    {
        return _dataService.GetData();
    }

    public List<EmissionsData> GetEmissionsDataForLocationByTime(string location, DateTime time, DateTime? toTime = null, int durationMinutes = 0)
    {
        return _dataService.GetData();
    }

    public List<EmissionsData> GetEmissionsDataForLocationsByTime(List<string> locations, DateTime time, DateTime? toTime = null, int durationMinutes = 0)
    {
        return _dataService.GetData();
    }
}
