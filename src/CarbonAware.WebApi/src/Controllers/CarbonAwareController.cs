using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using CarbonAware.Model;
using System.Diagnostics;

namespace CarbonAware.WebApi.Controllers;

[ApiController]
[Route("emissions")]
public class CarbonAwareController : ControllerBase
{
    private readonly ILogger<CarbonAwareController> _logger;
    private readonly ICarbonAwareAggregator _aggregator;
    private static readonly ActivitySource Activity = new ActivitySource(nameof(CarbonAwareController));

    public CarbonAwareController(ILogger<CarbonAwareController> logger, ICarbonAwareAggregator aggregator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _aggregator = aggregator ?? throw new ArgumentNullException(nameof(aggregator));
    }

    /// <summary>
    /// Calculate the best emission data by location for a specified time period.
    /// </summary>
    /// <returns>Array of EmissionsData objects that contains the location, time and the rating in g/kWh</returns>
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(EmissionsData))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [HttpGet("bylocations/best")]
    public async Task<IActionResult> GetBestEmissionsDataForLocationsByTime(string locations, DateTime? time = null, DateTime? toTime = null, int durationMinutes = 0)
    {
        using (var activity = Activity.StartActivity())
        {
            //The LocationType is hardcoded for now. Ideally this should be received from the request or configuration 
            var locationNames = locations.Split(',');
            IEnumerable<Location> locationEnumerable = locationNames.Select(location => new Location() { RegionName = location, LocationType = LocationType.CloudProvider });
            var props = new Dictionary<string, object?>() {
                { CarbonAwareConstants.Locations, locationEnumerable },
                { CarbonAwareConstants.Start, time},
                { CarbonAwareConstants.End, toTime },
                { CarbonAwareConstants.Duration, durationMinutes },
                { CarbonAwareConstants.Best, true }
            };

            _logger.LogInformation("Calling aggregator GetBestEmissionsDataAsync with payload {@props}", props);

            var response = await _aggregator.GetBestEmissionsDataAsync(props);
            return response != null ? Ok(response) : NoContent();
        }
    }


    /// <summary>
    /// Calculate the observed emission data by list of locations for a specified time period.
    /// </summary>
    /// <returns>Array of EmissionsData objects that contains the location, time and the rating in g/kWh</returns>
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<EmissionsData>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [HttpGet("bylocations")]
    public async Task<IActionResult> GetEmissionsDataForLocationsByTime(string locations, DateTime? time = null, DateTime? toTime = null, int durationMinutes = 0)
    {
        using (var activity = Activity.StartActivity())
        {
            var locationNames = locations.Split(',');
            IEnumerable<Location> locationEnumerable = locationNames.Select(location => new Location(){ RegionName = location, LocationType=LocationType.CloudProvider});
            var props = new Dictionary<string, object?>() {
                { CarbonAwareConstants.Locations, locationEnumerable },
                { CarbonAwareConstants.Start, time },
                { CarbonAwareConstants.End, toTime},
                { CarbonAwareConstants.Duration, durationMinutes },
            };
            
            return await GetEmissionsDataAsync(props);
        }
    }

    /// <summary>
    /// Calculate the best emission data by location for a specified time period.
    /// </summary>
    /// <returns>Array of EmissionsData objects that contains the location, time and the rating in g/kWh</returns>
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<EmissionsData>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [HttpGet("bylocation")]
    public async Task<IActionResult> GetEmissionsDataForLocationByTime([FromQuery, BindRequired] string location, DateTime? time = null, DateTime? toTime = null, int durationMinutes = 0)
    {
        using (var activity = Activity.StartActivity())
        {
            var locations = new List<Location>() { new Location() { RegionName = location, LocationType=LocationType.CloudProvider } };
            var props = new Dictionary<string, object?>() {
                { CarbonAwareConstants.Locations, locations },
                { CarbonAwareConstants.Start, time },
                { CarbonAwareConstants.End, toTime },
                { CarbonAwareConstants.Duration, durationMinutes },
            };
            
            return await GetEmissionsDataAsync(props);
        }
    }

    /// <summary>
    /// Maps user input query parameters to props dictionary for use with the data sources current forecast method.
    /// </summary>
    /// <param name="locations"> Comma-separated string of named locations.</param>
    /// <param name="startTime"> Start time of forecast period.</param>
    /// <param name="endTime"> End time of forecast period.</param>
    /// <param name="windowSize"> Size of rolling average window in minutes.</param>
    /// <returns>HTTP response containing the results of the data source current forecast call</returns>
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<EmissionsForecastDTO>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status501NotImplemented, Type = typeof(ValidationProblemDetails))]
    [HttpGet("forecasts/current")]
    public async Task<IActionResult> GetCurrentForecastData(string locations, DateTimeOffset? startTime = null, DateTimeOffset? endTime = null, int? windowSize = null)
    {
        using (var activity = Activity.StartActivity())
        {
            var locationNames = locations.Split(',');
            IEnumerable<Location> locationEnumerable = locationNames.Select(location => new Location(){ RegionName = location, LocationType=LocationType.CloudProvider});
            var props = new Dictionary<string, object?>() {
                { CarbonAwareConstants.Locations, locationEnumerable },
                { CarbonAwareConstants.Start, startTime },
                { CarbonAwareConstants.End, endTime },
                { CarbonAwareConstants.Duration, windowSize },
            };

            var forecasts = await _aggregator.GetCurrentForecastDataAsync(props);
            var results = forecasts.Select(f => EmissionsForecastDTO.FromEmissionsForecast(f));
            return Ok(results);
        }
    }

    /// <summary>
    /// Given a dictionary of properties, handles call to GetEmissionsDataAsync including logging and response handling.
    /// </summary>
    /// <param name="props"> Dictionary of properties to call plugin. </param>
    /// <returns>Result of the plugin call or resulting status response</returns>
    private async Task<IActionResult> GetEmissionsDataAsync(Dictionary<string, object?> props)
    {
        // NOTE: Any auth information would need to be redacted from logging
        _logger.LogInformation("Calling aggregator GetEmissionsDataAsync with payload {@props}", props);

        var response = await _aggregator.GetEmissionsDataAsync(props);
        return response.Any() ? Ok(response) : NoContent();
    }
}
