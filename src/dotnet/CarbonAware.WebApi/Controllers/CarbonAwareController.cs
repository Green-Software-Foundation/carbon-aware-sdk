using Microsoft.AspNetCore.Mvc;
using CarbonAware.Config;

namespace CarbonAware.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CarbonAwareController : ControllerBase
    {
        private readonly ILogger<CarbonAwareController> _logger;
        private ICarbonAwarePlugin _plugin;
        private ServiceManager _serviceManager;

        public CarbonAwareController(ILogger<CarbonAwareController> logger)
        {
            _logger = logger;
            _serviceManager = new ServiceManager("carbon-aware.json");
            _plugin = _serviceManager.ServiceProvider.GetService<ICarbonAwarePlugin>();
        }

        [HttpPost("GetBestEmissionsDataForLocationsByTime")]
        public IEnumerable<EmissionsData> GetBestEmissionsDataForLocationsByTime(List<string> locations, DateTime? time = null, DateTime? toTime = null)
        {
            var response = _plugin.GetBestEmissionsDataForLocationsByTime(locations, time ?? DateTime.Now, toTime);

            return response;
        }

        [HttpPost("GetEmissionsDataForLocationsByTime")]
        public IEnumerable<EmissionsData> GetEmissionsDataForLocationsByTime(List<string> locations, DateTime? time = null, DateTime? toTime = null)
        {
            var response = _plugin.GetEmissionsDataForLocationsByTime(locations, time ?? DateTime.Now, toTime);
       
            return response;
        }

        [HttpGet("GetEmissionsDataForLocationByTime")]
        public IEnumerable<EmissionsData> GetEmissionsDataForLocationByTime(string location, DateTime? time = null, DateTime? toTime = null)
        {
            var response = _plugin.GetEmissionsDataForLocationByTime(location, time ?? DateTime.Now, toTime);

            return response;
        }
    }
}