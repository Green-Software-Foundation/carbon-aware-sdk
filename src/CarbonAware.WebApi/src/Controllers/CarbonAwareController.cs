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
    /// <param name="locations"> String array of named locations.</param>
    /// <param name="time"> [Optional] Start time for the data query.</param>
    /// <param name="toTime"> [Optional] End time for the data query.</param>
    /// <param name="durationMinutes"> [Optional] Duration for the data query.</param>
    /// <returns>Array of EmissionsData objects that contains the location, time and the rating in g/kWh</returns>
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(EmissionsData))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [HttpGet("bylocations/best")]
    public async Task<IActionResult> GetBestEmissionsDataForLocationsByTime([FromQuery(Name = "location"), BindRequired] string[] locations, DateTime? time = null, DateTime? toTime = null, int durationMinutes = 0)
    {
        using (var activity = Activity.StartActivity())
        {
            //The LocationType is hardcoded for now. Ideally this should be received from the request or configuration 
            IEnumerable<Location> locationEnumerable = CreateLocationsFromQueryString(locations);
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
    /// <param name="locations"> String array of named locations.</param>
    /// <param name="time"> [Optional] Start time for the data query.</param>
    /// <param name="toTime"> [Optional] End time for the data query.</param>
    /// <param name="durationMinutes"> [Optional] Duration for the data query.</param>
    /// <returns>Array of EmissionsData objects that contains the location, time and the rating in g/kWh</returns>
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<EmissionsData>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [HttpGet("bylocations")]
    public async Task<IActionResult> GetEmissionsDataForLocationsByTime([FromQuery(Name = "location"), BindRequired] string[] locations, DateTime? time = null, DateTime? toTime = null, int durationMinutes = 0)
    {
        using (var activity = Activity.StartActivity())
        {
            IEnumerable<Location> locationEnumerable = CreateLocationsFromQueryString(locations);
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
    /// <param name="location"> String named location.</param>
    /// <param name="time"> [Optional] Start time for the data query.</param>
    /// <param name="toTime"> [Optional] End time for the data query.</param>
    /// <param name="durationMinutes"> [Optional] Duration for the data query.</param>
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
            var locations = new List<Location>() { new Location() { RegionName = location, LocationType = LocationType.CloudProvider } };
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
    /// <param name="locations"> String array of named locations.</param>
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
    public async Task<IActionResult> GetCurrentForecastData([FromQuery(Name = "location"), BindRequired] string[] locations, DateTimeOffset? startTime = null, DateTimeOffset? endTime = null, int? windowSize = null)
    {
        using (var activity = Activity.StartActivity())
        {
            IEnumerable<Location> locationEnumerable = CreateLocationsFromQueryString(locations);
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
    /// Given an array of requested historical forecasts, retrieve the forecasted data and calculate the optimal
    /// marginal carbon intensity window. 
    /// </summary>
    /// <remarks>
    /// This endpoint takes a batch of requests for historical forecast data, fetches them, and calculates the optimal 
    /// marginal carbon intensity windows for each using the same parameters available to the '/emissions/forecasts/current'
    /// endpoint.
    ///
    /// The forecast data represents what the data source predicted future marginal carbon intesity values to be at that 
    /// time, not the measured emissions data that actually occured.
    ///
    /// This endpoint is useful for back-testing what one might have done in the past, if they had access to the 
    /// current forecast at the time.
    /// </remarks>
    /// <param name="requestedForecasts"> Array of requested forecasts.</param>
    /// <returns>An array of forecasts with their optimal marginal carbon intensity window.</returns>
    /// <response code="200">Returns the requested forecast objects</response>
    /// <response code="400">Returned if any of the requested items are invalid</response>
    /// <response code="500">Internal server error</response>
    /// <response code="501">Returned if the underlying data source does not support forecasting</response>
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<EmissionsForecastDTO>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status501NotImplemented, Type = typeof(ValidationProblemDetails))]
    [HttpPost("forecasts/batch")]
    public IActionResult BatchForecastData(IEnumerable<EmissionsForecastBatchDTO> requestedForecasts)
    {
        // Dummy result.
        // TODO: implement this controller method after spec is approved.
        var result = new List<EmissionsForecastDTO>();
        return Ok(result);
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

    private IEnumerable<Location> CreateLocationsFromQueryString(string[] queryStringLocations)
    {
        var locations = queryStringLocations
            .Where(location => !String.IsNullOrEmpty(location))
            .Select(location => new Location() { RegionName = location, LocationType = LocationType.CloudProvider });

        return locations.Any() ? locations : throw new ArgumentException("Required field: A value for 'location' must be provided.");
    }

}
