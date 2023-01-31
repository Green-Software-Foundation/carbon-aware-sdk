using GSF.CarbonAware.Handlers;
using GSF.CarbonAware.Models;
using Microsoft.AspNetCore.Mvc;

namespace CarbonAware.WebApi.Controllers;

[ApiController]
[Route("locations")]
public class LocationsController : ControllerBase
{
    private readonly ILogger<CarbonAwareController> _logger;

    private readonly ILocationHandler _locationSource;


    public LocationsController(ILogger<CarbonAwareController> logger, ILocationHandler locationSouce)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _locationSource = locationSouce ?? throw new ArgumentNullException(nameof(locationSouce));
    }

    /// <summary>
    /// Get all locations instances
    /// </summary>
    /// <returns>Dictionary with <see cref="Location"/> instances</returns>
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IDictionary<string, Location>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [HttpGet()]
    public async Task<IActionResult> GetAllLocations()
    {
        var response = await _locationSource.GetLocationsAsync(); 
        return response.Any() ? Ok(response) : NoContent();
    }
}
