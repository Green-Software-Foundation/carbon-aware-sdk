using Microsoft.AspNetCore.Mvc;
using CarbonAware.Config;
using CarbonAware.Data;

namespace CarbonAware.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CarbonAwareController : ControllerBase
    {
        private readonly ILogger<CarbonAwareController> _logger;
        private ICarbonAwarePlugin _plugin;
        private ServiceManager _serviceManager;
        private IConfigManager _configManager;

        public CarbonAwareController(ILogger<CarbonAwareController> logger)
        {
            _logger = logger;

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

        [HttpPost("GetBestEmissionsDataForLocationsByTime")]
        public IEnumerable<EmissionsData> GetBestEmissionsDataForLocationsByTime(List<string> locations, DateTime? time = null, DateTime? toTime = null, TimeSpan? duration = null)
        {
            var response = _plugin.GetBestEmissionsDataForLocationsByTime(locations, time ?? DateTime.Now, toTime);

            return response;
        }

        [HttpPost("GetEmissionsDataForLocationsByTime")]
        public IEnumerable<EmissionsData> GetEmissionsDataForLocationsByTime(List<string> locations, DateTime? time = null, DateTime? toTime = null, TimeSpan? duration = null)
        {
            var response = _plugin.GetEmissionsDataForLocationsByTime(locations, time ?? DateTime.Now, toTime, duration);
       
            return response;
        }

        [HttpGet("GetEmissionsDataForLocationByTime")]
        public IEnumerable<EmissionsData> GetEmissionsDataForLocationByTime(string location, DateTime? time = null, DateTime? toTime = null, TimeSpan? duration = null)
        {
            var response = _plugin.GetEmissionsDataForLocationByTime(location, time ?? DateTime.Now, toTime, duration);

            return response;
        }
    }
}