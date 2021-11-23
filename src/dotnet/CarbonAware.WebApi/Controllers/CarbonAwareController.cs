using Microsoft.AspNetCore.Mvc;
using CarbonAware.Plugins.BasicJsonPlugin;

namespace CarbonAware.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CarbonAwareController : ControllerBase
    {
        private readonly ILogger<CarbonAwareController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public CarbonAwareController(ILogger<CarbonAwareController> logger)
        {
            _logger = logger;
        }

        [HttpPost("GetBestEmissionsDataForLocationsByTime")]
        public IEnumerable<EmissionsData> GetBestEmissionsDataForLocationsByTime(List<string> locations, DateTime time, DateTime? toTime = null)
        {
            var plugin = GetPlugin();

            var response = plugin.GetBestEmissionsDataForLocationsByTime(locations, time, toTime);

            return response;
        }

        [HttpPost("GetEmissionsDataForLocationsByTime")]
        public IEnumerable<EmissionsData> GetEmissionsDataForLocationsByTime(List<string> locations, DateTime time, DateTime? toTime = null)
        {
            var plugin = GetPlugin();

            var response = plugin.GetEmissionsDataForLocationsByTime(locations, time, toTime);
       
            return response;
        }

        [HttpGet("GetEmissionsDataForLocationByTime")]
        public IEnumerable<EmissionsData> GetEmissionsDataForLocationByTime(string location, DateTime time, DateTime? toTime = null)
        {
            var plugin = GetPlugin();

            var response = plugin.GetEmissionsDataForLocationByTime(location, time, toTime);

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