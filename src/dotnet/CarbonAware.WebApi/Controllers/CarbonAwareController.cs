using Microsoft.AspNetCore.Mvc;
using CarbonAware.Plugins.BasicJsonPlugin;

namespace CarbonAware.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CarbonAwareController : ControllerBase
    {
        private readonly ILogger<CarbonAwareController> _logger;
        private ICarbonAwarePlugin _plugin;
        public CarbonAwareController(ILogger<CarbonAwareController> logger)
        {
            _logger = logger;
            _plugin = GetPlugin();
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

        private static CarbonAwareBasicDataPlugin GetPlugin()
        {
            var dataService = new CarbonAwareStaticJsonDataService();
            var filePath = Path.Combine("data-files", "dummy-data-azure-emissions.json");

            dataService.SetFileName(filePath);
            var plugin = new CarbonAwareBasicDataPlugin(dataService);
            return plugin;
        }
    }
}