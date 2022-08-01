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
    ///   Retrieves the most recent forecasted data and calculates the optimal marginal carbon intensity window.
    /// </summary>
    /// <param name="locations"> String array of named locations.</param>
    /// <param name="dataStartAt">
    ///   Start time boundary of forecasted data points. Ignores current forecast data points before this time.
    ///   Defaults to the earliest time in the forecast data.
    /// </param>
    /// <param name="dataEndAt">
    ///   End time boundary of forecasted data points. Ignores current forecast data points after this time.
    ///   Defaults to the latest time in the forecast data.
    /// </param>
    /// <param name="windowSize">
    ///   The estimated duration (in minutes) of the workload.
    ///   Defaults to the duration of a single forecast data point.
    /// </param>
    /// <remarks>
    ///   This endpoint fetches only the most recently generated forecast for all provided locations.  It uses the "dataStartAt" and 
    ///   "dataEndAt" parameters to scope the forecasted data points (if available for those times). If no start or end time 
    ///   boundaries are provided, the entire forecast dataset is used. The scoped data points are used to calculate average marginal 
    ///   carbon intensities of the specified "windowSize" and the optimal marginal carbon intensity window is identified.
    ///
    ///   The forecast data represents what the data source predicts future marginal carbon intesity values to be, 
    ///   not actual measured emissions data (as future values cannot be known).
    ///
    ///   This endpoint is useful for determining if there is a more carbon-optimal time to use electicity predicted in the future.
    /// </remarks>
    /// <returns>An array of forecasts (one per requested location) with their optimal marginal carbon intensity windows.</returns>
    /// <response code="200">Returns the requested forecast objects</response>
    /// <response code="400">Returned if any of the input parameters are invalid</response>
    /// <response code="500">Internal server error</response>
    /// <response code="501">Returned if the underlying data source does not support forecasting</response>
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<EmissionsForecastDTO>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status501NotImplemented, Type = typeof(ValidationProblemDetails))]
    [HttpGet("forecasts/current")]
    public async Task<IActionResult> GetCurrentForecastData([FromQuery(Name = "location"), BindRequired] string[] locations, DateTimeOffset? dataStartAt = null, DateTimeOffset? dataEndAt = null, int? windowSize = null)
    {
        using (var activity = Activity.StartActivity())
        {
            IEnumerable<Location> locationEnumerable = CreateLocationsFromQueryString(locations);
            var props = new Dictionary<string, object?>() {
                { CarbonAwareConstants.Locations, locationEnumerable },
                { CarbonAwareConstants.Start, dataStartAt },
                { CarbonAwareConstants.End, dataEndAt },
                { CarbonAwareConstants.Duration, windowSize },
            };

            var forecasts = await _aggregator.GetCurrentForecastDataAsync(props);
            var results = forecasts.Select(f => EmissionsForecastDTO.FromEmissionsForecast(f));
            return Ok(results);
        }
    }

    /// <summary>
    /// Given an array of historical forecasts, retrieves the data that contains
    /// forecasts metadata, the optimal forecast and a range of forecasts filtered by the attributes [start...end] if provided.
    /// </summary>
    /// <remarks>
    /// This endpoint takes a batch of requests for historical forecast data, fetches them, and calculates the optimal 
    /// marginal carbon intensity windows for each using the same parameters available to the '/emissions/forecasts/current'
    /// endpoint.
    ///
    /// This endpoint is useful for back-testing what one might have done in the past, if they had access to the 
    /// current forecast at the time.
    /// </remarks>
    /// <param name="requestedForecasts"> Array of requested forecasts.</param>
    /// <returns>An array of forecasts with their optimal marginal carbon intensity window.</returns>
    /// <response code="200">Returns the requested forecast objects</response>
    /// <response code="400">Returned if any of the input parameters are invalid</response>
    /// <response code="500">Internal server error</response>
    /// <response code="501">Returned if the underlying data source does not support forecasting</response>
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ValidationProblemDetails))]
    [HttpPost("forecasts/batch")]
    public async IAsyncEnumerable<EmissionsForecastDTO> BatchForecastDataAsync(IEnumerable<EmissionsForecastBatchDTO> requestedForecasts)
    {
        using (var activity = Activity.StartActivity())
        {
            foreach (var forecastBatchDTO in requestedForecasts)
            {
                IEnumerable<Location> locationEnumerable = CreateLocationsFromQueryString(new string[] { forecastBatchDTO.Location! });
                var props = new Dictionary<string, object?>() {
                    { CarbonAwareConstants.Locations, locationEnumerable },
                    { CarbonAwareConstants.Start, forecastBatchDTO.DataStartAt },
                    { CarbonAwareConstants.End, forecastBatchDTO.DataEndAt },
                    { CarbonAwareConstants.Duration, forecastBatchDTO.WindowSize },
                    { CarbonAwareConstants.ForecastRequestedAt, forecastBatchDTO.RequestedAt },
                };
                // NOTE: Current Error Handling done by HttpResponseExceptionFilter can't handle exceptions
                // thrown by the underline framework for this method, therefore all exceptions are handled as 500.
                // Refactoring with a middleware exception handler should cover this use case too.
                var forecast = await _aggregator.GetForecastDataAsync(props);
                yield return EmissionsForecastDTO.FromEmissionsForecast(forecast);
            }
        }
    }

    /// <summary>
    /// Retrieves the measured carbon intensity data between the time boundaries and calculates the average carbon intensity during that period. 
    /// </summary>
    /// <remarks>
    ///  This endpoint is useful for reporting the measured carbon intensity for a specific time period in a specific location.
    /// </remarks>
    /// <param name="location">The location name of the region that we are measuring carbon usage in. </param>
    /// <param name="startTime">The time at which the workload and corresponding carbon usage begins.</param>
    /// <param name="endTime">The time at which the workload and corresponding carbon usage ends. </param>
    /// <returns>A single object that contains the location, time boundaries and average carbon intensity value.</returns>
    /// <response code="200">Returns a single object that contains the information about the request and the average marginal carbon intensity</response>
    /// <response code="400">Returned if any of the requested items are invalid</response>
    /// <response code="500">Internal server error</response>
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CarbonIntensityDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ValidationProblemDetails))]
    [HttpGet("average-carbon-intensity")]
    public async Task<IActionResult> GetAverageCarbonIntensity([FromQuery, BindRequired] string location, [FromQuery, BindRequired] DateTimeOffset startTime, [FromQuery, BindRequired] DateTimeOffset endTime)
    {
        using (var activity = Activity.StartActivity())
        {
            IEnumerable<Location> locationEnumerable = CreateLocationsFromQueryString(new string[] { location });
            var props = new Dictionary<string, object?>() {
                { CarbonAwareConstants.Locations, locationEnumerable },
                { CarbonAwareConstants.Start, startTime },
                { CarbonAwareConstants.End, endTime },
            };

            var result = await this._aggregator.CalculateAverageCarbonIntensityAsync(props);

            CarbonIntensityDTO carbonIntensity = new CarbonIntensityDTO
            {
                Location = location,
                StartTime = startTime,
                EndTime = endTime,
                CarbonIntensity = result,
            };
            _logger.LogDebug("calculated average carbon intensity: {carbonIntensity}", carbonIntensity);
            return Ok(carbonIntensity);
        }
    }


    /// <summary>
    /// Given an array of request objects, each with their own location and time boundaries, calculate the average carbon intensity for that location and time period 
    /// and return an array of carbon intensity objects.
    /// </summary>
    /// <remarks>
    /// The application only supports batching across a single location with different time boundaries. If multiple locations are provided, an error is returned.
    /// For each item in the request array, the application returns a corresponding object containing the location, time boundaries, and average marginal carbon intensity. 
    /// </remarks>
    /// <param name="requestedCarbonIntensities"> Array of inputs where each contains a "location", "startDate", and "endDate" for which to calculate average marginal carbon intensity. </param>
    /// <returns>An array of CarbonIntensityDTO objects which each have a location, start time, end time, and the average marginal carbon intensity over that time period.</returns>
    /// <response code="200">Returns an array of objects where each contains location, time boundaries and the corresponding average marginal carbon intensity</response>
    /// <response code="400">Returned if any of the requested items are invalid</response>
    /// <response code="500">Internal server error</response>
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ValidationProblemDetails))]
    [HttpPost("average-carbon-intensity/batch")]
    public async IAsyncEnumerable<CarbonIntensityDTO> GetAverageCarbonIntensityBatch(IEnumerable<CarbonIntensityBatchDTO> requestedCarbonIntensities)
    {
        using (var activity = Activity.StartActivity())
        {
            foreach (var carbonIntensityBatchDTO in requestedCarbonIntensities)
            {
                IEnumerable<Location> locationEnumerable = CreateLocationsFromQueryString(new string[] { carbonIntensityBatchDTO.Location! });
                var props = new Dictionary<string, object?>() {
                    { CarbonAwareConstants.Locations, locationEnumerable },
                    { CarbonAwareConstants.Start, carbonIntensityBatchDTO.StartTime },
                    { CarbonAwareConstants.End, carbonIntensityBatchDTO.EndTime }
                };
                // NOTE: Current Error Handling done by HttpResponseExceptionFilter can't handle exceptions
                // thrown by the underline framework for this method, therefore all exceptions are handled as 500.
                // Refactoring with a middleware exception handler should cover this use case too.
                var carbonIntensity = await this._aggregator.CalculateAverageCarbonIntensityAsync(props);
                CarbonIntensityDTO carbonIntensityDTO = new CarbonIntensityDTO
                {
                    Location = carbonIntensityBatchDTO.Location,
                    StartTime = carbonIntensityBatchDTO.StartTime,
                    EndTime = carbonIntensityBatchDTO.EndTime,
                    CarbonIntensity = carbonIntensity,
                };
                yield return carbonIntensityDTO;
            }
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

    private IEnumerable<Location> CreateLocationsFromQueryString(string[] queryStringLocations)
    {
        var locations = queryStringLocations
            .Where(location => !String.IsNullOrEmpty(location))
            .Select(location => new Location() { RegionName = location, LocationType = LocationType.CloudProvider });

        return locations.Any() ? locations : throw new ArgumentException("Required field: A value for 'location' must be provided.");
    }

}
