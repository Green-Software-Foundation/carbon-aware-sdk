using CarbonAware.Interfaces;
using CarbonAware.Model;
using Microsoft.AspNetCore.Mvc;

namespace CarbonAware.WebApi.Controllers;

[ApiController]
[Route("locations")]
public class LocationsController : ControllerBase
{
    private readonly ILogger<CarbonAwareController> _logger;

    private readonly ILocationSource _locationSource;


    public LocationsController(ILogger<CarbonAwareController> logger, ILocationSource locationSouce)
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
        var response = await _locationSource.GetGeopositionLocationsAsync(); 
        return response.Any() ? Ok(response) : NoContent();
    }
}
