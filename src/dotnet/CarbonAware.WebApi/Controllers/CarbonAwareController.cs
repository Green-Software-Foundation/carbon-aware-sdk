using CarbonAware.Config;
using CarbonAware.Model;
using Microsoft.AspNetCore.Mvc;
using HttpGetAttribute = Microsoft.AspNetCore.Mvc.HttpGetAttribute;

namespace CarbonAware.WebApi.Controllers;

[ApiController]
[Microsoft.AspNetCore.Mvc.Route("emissions")]
public class CarbonAwareController : ControllerBase
{
    private readonly ILogger<CarbonAwareController> _logger;
    private ICarbonAwarePlugin _plugin;
    private ServiceManager _serviceManager;
    private IConfigManager _configManager;

    private readonly ICarbonAware NewPlugin;

    public CarbonAwareController(ILogger<CarbonAwareController> logger, ICarbonAware newPlugin)
    {
        _logger = logger;
        NewPlugin = newPlugin ?? throw new ArgumentNullException(nameof(newPlugin));

        _configManager = new ConfigManager("carbon-aware.json");
        _serviceManager = new ServiceManager(_configManager);
        var pluginService = _serviceManager.ServiceProvider.GetService<ICarbonAwarePlugin>();

        if (pluginService is not null)
        {
            _plugin = pluginService;
        }
        else
        {
            throw new Exception("Services are not configured properly.  Could not find plugin service.");
        }
    }

    [HttpGet("bylocations/best")]
    public IEnumerable<EmissionsData> GetBestEmissionsDataForLocationsByTime([FromQuery(Name = "locations")] string[] locations, DateTime? time = null, DateTime? toTime = null, int durationMinutes = 0)
    {
        var response = _plugin.GetBestEmissionsDataForLocationsByTime(locations.ToList(), time ?? DateTime.Now, toTime, durationMinutes);

        return response;
    }

    [HttpGet("bylocations")]
    public IEnumerable<EmissionsData> GetEmissionsDataForLocationsByTime([FromQuery(Name = "locations")] string[] locations, DateTime? time = null, DateTime? toTime = null, int durationMinutes = 0)
    {
        var response = _plugin.GetEmissionsDataForLocationsByTime(locations.ToList(), time ?? DateTime.Now, toTime, durationMinutes);

        return response;
    }

    [HttpGet("bylocation")]
    public IEnumerable<EmissionsData> GetEmissionsDataForLocationByTime(string location, DateTime? time = null, DateTime? toTime = null, int durationMinutes = 0)
    {
        var response = _plugin.GetEmissionsDataForLocationByTime(location, time ?? DateTime.Now, toTime, durationMinutes);

        return response;
    }

    [HttpGet("newdata")]
    public async Task<IActionResult> GetNewEmissionsAsync()
    {
        // Example of using filter
        var props = new Dictionary<string, string>() {
            { "firstkey", "firstvalue" },
            { "secondkey", "secondvalue" }
        };
        // faking data that might come from HttpHeaders for instance
        var headerLoc = "eastus";
        var headerRating = 5;

        var data = await NewPlugin.GetEmissionsDataAsync(props, (x) => {
            return x.Location == headerLoc && x.Rating < headerRating;
        });
        return Ok(data);
    }
}
