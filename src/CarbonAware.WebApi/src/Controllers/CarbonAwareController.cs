using CarbonAware.WebApi.Models;
using GSF.CarbonAware.Handlers;
using GSF.CarbonAware.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Diagnostics;

namespace CarbonAware.WebApi.Controllers;

[ApiController]
[Route("emissions")]
public class CarbonAwareController : ControllerBase
{
    private readonly ILogger<CarbonAwareController> _logger;
    
    private readonly IEmissionsHandler _emissionsHandler;

    private readonly IForecastHandler _forecastHandler;

    private readonly ILocationHandler _locationSource;

    private static readonly ActivitySource Activity = new ActivitySource(nameof(CarbonAwareController));

    public CarbonAwareController(ILogger<CarbonAwareController> logger, IEmissionsHandler emissionsHandler, IForecastHandler forecastHandler, ILocationHandler locationSouce)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _forecastHandler = forecastHandler ?? throw new ArgumentNullException(nameof(forecastHandler));
        _emissionsHandler = emissionsHandler ?? throw new ArgumentNullException(nameof(emissionsHandler));
        _locationSource = locationSouce ?? throw new ArgumentNullException(nameof(locationSouce));
    }

    /// <summary>
    /// Calculate the best emission data by list of locations for a specified time period.
    /// </summary>
    /// <param name="parameters">The request object <see cref="EmissionsDataForLocationsParametersDTO"/></param>
    /// <returns>Array of EmissionsData objects that contains the location, time and the rating in g/kWh</returns>
    [Produces("application/json", "application/json; charset=utf-8")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<EmissionsData>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [HttpGet("bylocations/best")]
    public async Task<IActionResult> GetBestEmissionsDataForLocationsByTime([FromQuery] EmissionsDataForLocationsParametersDTO parameters)
    {
        var response = await _emissionsHandler.GetBestEmissionsDataAsync(parameters.MultipleLocations!, parameters.Start, parameters.End);
        return response.Any() ? Ok(response) : NoContent();
    }

    /// <summary>
    /// Calculate the observed emission data by list of locations for a specified time period.
    /// </summary>
    /// <param name="parameters">The request object <see cref="EmissionsDataForLocationsParametersDTO"/></param>
    /// <returns>Array of EmissionsData objects that contains the location, time and the rating in g/kWh</returns>
    [Produces("application/json", "application/json; charset=utf-8")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<EmissionsData>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [HttpGet("bylocations")]
    public async Task<IActionResult> GetEmissionsDataForLocationsByTime([FromQuery] EmissionsDataForLocationsParametersDTO parameters)
    {
        var response = await _emissionsHandler.GetEmissionsDataAsync(parameters.MultipleLocations!, parameters.Start, parameters.End);
        return response.Any() ? Ok(response) : NoContent();
    }

    /// <summary>
    /// Calculate the best emission data by location for a specified time period.
    /// </summary>
    /// <param name="location"> String named location.</param>
    /// <param name="startTime"> [Optional] Start time for the data query.</param>
    /// <param name="endTime"> [Optional] End time for the data query.</param>
    /// <returns>Array of EmissionsData objects that contains the location, time and the rating in g/kWh</returns>
    [Produces("application/json", "application/json; charset=utf-8")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<EmissionsData>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [HttpGet("bylocation")]
    public async Task<IActionResult> GetEmissionsDataForLocationByTime(
        [FromQuery, SwaggerParameter(Required = true)] string location,
        [FromQuery(Name = "time")] DateTimeOffset? startTime = null,
        [FromQuery(Name = "toTime")] DateTimeOffset? endTime = null)
    {
        var parameters = new EmissionsDataForLocationsParametersDTO
        {
            MultipleLocations = new string[]{ location },
            Start = startTime,
            End = endTime
        };
        return await GetEmissionsDataForLocationsByTime(parameters);
    }

    /// <summary>
    ///   Retrieves the most recent forecasted data and calculates the optimal carbon intensity window.
    /// </summary>
    /// <param name="parameters">The request object <see cref="EmissionsForecastCurrentParametersDTO"/></param>
    /// <remarks>
    ///   This endpoint fetches only the most recently generated forecast for all provided locations.  It uses the "dataStartAt" and 
    ///   "dataEndAt" parameters to scope the forecasted data points (if available for those times). If no start or end time 
    ///   boundaries are provided, the entire forecast dataset is used. The scoped data points are used to calculate average 
    ///   carbon intensities of the specified "windowSize" and the optimal carbon intensity window is identified.
    ///
    ///   The forecast data represents what the data source predicts future carbon intensity values to be, 
    ///   not actual measured emissions data (as future values cannot be known).
    ///
    ///   This endpoint is useful for determining if there is a more carbon-optimal time to use electricity predicted in the future.
    /// </remarks>
    /// <returns>An array of forecasts (one per requested location) with their optimal carbon intensity windows.</returns>
    /// <response code="200">Returns the requested forecast objects</response>
    /// <response code="400">Returned if any of the input parameters are invalid</response>
    /// <response code="500">Internal server error</response>
    /// <response code="501">Returned if the underlying data source does not support forecasting</response>
    [Produces("application/json", "application/json; charset=utf-8")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<EmissionsForecastDTO>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status501NotImplemented, Type = typeof(ValidationProblemDetails))]
    [HttpGet("forecasts/current")]
    public async Task<IActionResult> GetCurrentForecastData([FromQuery] EmissionsForecastCurrentParametersDTO parameters)
    {
        var forecasts = await _forecastHandler.GetCurrentForecastAsync(parameters.MultipleLocations!, parameters.Start, parameters.End, parameters.Duration);
        var results = forecasts.Select(f => EmissionsForecastDTO.FromEmissionsForecast(f, parameters.Requested, parameters.Start, parameters.End, parameters.Duration));
        return Ok(results);
    }

    /// <summary>
    /// Given an array of historical forecasts, retrieves the data that contains
    /// forecasts metadata, the optimal forecast and a range of forecasts filtered by the attributes [start...end] if provided.
    /// </summary>
    /// <remarks>
    /// This endpoint takes a batch of requests for historical forecast data, fetches them, and calculates the optimal 
    /// carbon intensity windows for each using the same parameters available to the '/emissions/forecasts/current'
    /// endpoint.
    ///
    /// This endpoint is useful for back-testing what one might have done in the past, if they had access to the 
    /// current forecast at the time.
    /// </remarks>
    /// <param name="requestedForecasts"> Array of requested forecasts.</param>
    /// <returns>An array of forecasts with their optimal carbon intensity window.</returns>
    /// <response code="200">Returns the requested forecast objects</response>
    /// <response code="400">Returned if any of the input parameters are invalid</response>
    /// <response code="500">Internal server error</response>
    /// <response code="501">Returned if the underlying data source does not support forecasting</response>
    [Produces("application/json", "application/json; charset=utf-8")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<EmissionsForecastDTO>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ValidationProblemDetails))]
    [HttpPost("forecasts/batch")]
    public async Task<IActionResult> BatchForecastDataAsync([FromBody] IEnumerable<EmissionsForecastBatchParametersDTO> requestedForecasts)
    {
        var result = new List<EmissionsForecastDTO>();
        foreach ( var forecastParameters in requestedForecasts)
        {
            var forecast = await _forecastHandler.GetForecastByDateAsync(
                forecastParameters.SingleLocation!,
                forecastParameters.Start,
                forecastParameters.End,
                forecastParameters.Requested,
                forecastParameters.Duration
            );
            result.Add(EmissionsForecastDTO.FromEmissionsForecast(forecast, forecastParameters.Requested, forecastParameters.Start, forecastParameters.End, forecastParameters.Duration));
        };

        return Ok(result);
    }

    /// <summary>
    /// Retrieves the measured carbon intensity data between the time boundaries and calculates the average carbon intensity during that period. 
    /// </summary>
    /// <remarks>
    ///  This endpoint is useful for reporting the measured carbon intensity for a specific time period in a specific location.
    /// </remarks>
    /// <param name="parameters">The request object <see cref="CarbonIntensityParametersDTO"/></param>
    /// <returns>A single object that contains the location, time boundaries and average carbon intensity value.</returns>
    /// <response code="200">Returns a single object that contains the information about the request and the average carbon intensity</response>
    /// <response code="400">Returned if any of the requested items are invalid</response>
    /// <response code="500">Internal server error</response>
    [Produces("application/json", "application/json; charset=utf-8")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CarbonIntensityDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ValidationProblemDetails))]
    [HttpGet("average-carbon-intensity")]
    public async Task<IActionResult> GetAverageCarbonIntensity([FromQuery] CarbonIntensityParametersDTO parameters)
    {
        var result = await this._emissionsHandler.GetAverageCarbonIntensityAsync(
            parameters.SingleLocation!,
            (DateTimeOffset)parameters.Start!,
            (DateTimeOffset)parameters.End!
        );
        
        var carbonIntensity = new CarbonIntensityDTO
        {
            Location = parameters.SingleLocation,
            StartTime = parameters.Start,
            EndTime = parameters.End,
            CarbonIntensity = result,
        };
        _logger.LogDebug("calculated average carbon intensity: {carbonIntensity}", carbonIntensity);
        return Ok(carbonIntensity);
    }

    /// <summary>
    /// Given an array of request objects, each with their own location and time boundaries, calculate the average carbon intensity for that location and time period 
    /// and return an array of carbon intensity objects.
    /// </summary>
    /// <remarks>
    /// The application only supports batching across a single location with different time boundaries. If multiple locations are provided, an error is returned.
    /// For each item in the request array, the application returns a corresponding object containing the location, time boundaries, and average carbon intensity. 
    /// </remarks>
    /// <param name="requestedCarbonIntensities"> Array of inputs where each contains a "location", "startDate", and "endDate" for which to calculate average carbon intensity. </param>
    /// <returns>An array of CarbonIntensityDTO objects which each have a location, start time, end time, and the average carbon intensity over that time period.</returns>
    /// <response code="200">Returns an array of objects where each contains location, time boundaries and the corresponding average carbon intensity</response>
    /// <response code="400">Returned if any of the requested items are invalid</response>
    /// <response code="500">Internal server error</response>
    [Produces("application/json", "application/json; charset=utf-8")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CarbonIntensityDTO>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ValidationProblemDetails))]
    [HttpPost("average-carbon-intensity/batch")]
    public async Task<IActionResult> GetAverageCarbonIntensityBatch([FromBody] IEnumerable<CarbonIntensityBatchParametersDTO> requestedCarbonIntensities)
    {
        var result = new List<CarbonIntensityDTO>();
        foreach (var carbonIntensityBatchDTO in requestedCarbonIntensities)
        {
            var carbonIntensityValue = await this._emissionsHandler.GetAverageCarbonIntensityAsync(
                carbonIntensityBatchDTO.SingleLocation!,
                (DateTimeOffset)carbonIntensityBatchDTO.Start!,
                (DateTimeOffset)carbonIntensityBatchDTO.End!
            );
            
            var carbonIntensityDTO = new CarbonIntensityDTO()
            {
                Location = carbonIntensityBatchDTO.SingleLocation,
                StartTime = carbonIntensityBatchDTO.Start,
                EndTime = carbonIntensityBatchDTO.End,
                CarbonIntensity = carbonIntensityValue,
            };
            result.Add(carbonIntensityDTO);
        }

        return Ok(result);
    }
}
