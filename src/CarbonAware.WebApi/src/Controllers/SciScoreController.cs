using CarbonAware.Aggregators.SciScore;
using CarbonAware.WebApi.Models;
using CarbonAware.Model;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using System.Diagnostics;

namespace CarbonAware.WebApi.Controllers;

/// <summary>
/// Controller for the API routes that lead to retrieving the sci scores and marginal carbon intensities 
/// </summary>
[Route("sci-scores")]
[ApiController]
public class SciScoreController : ControllerBase
{
    private readonly ILogger<SciScoreController> _logger;
    private readonly ISciScoreAggregator _aggregator;

    private readonly ActivitySource _activitySource;

    public SciScoreController(ILogger<SciScoreController> logger, ISciScoreAggregator aggregator, ActivitySource activitySource)
    {
        _logger = logger;
        _aggregator = aggregator;
        _activitySource = activitySource;
    }

    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]

    /// <summary> Gets sci-score value, currently dummy function to keep consistency </summary>
    /// <param name="input"> input from JSON request converted to input object with location and time interval </param>
    /// <returns>Result of the call to the aggregator that calculates the sci score</returns>

    public Task<IActionResult> CreateAsync(SciScoreInput input)
    {
        _logger.LogDebug("calculate sciscore with input: {input}", input);

        var score = new SciScore
        {
            SciScoreValue = 100.0,
            EnergyValue = 1.0,
            MarginalCarbonIntensityValue = 100.0,
            EmbodiedEmissionsValue = 0.0,
            FunctionalUnitValue = 1
        };
        _logger.LogDebug("calculated sciscore values: {score}", score);
        return Task.FromResult<IActionResult>(Ok(score));
    }

    [HttpPost("marginal-carbon-intensity")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    /// <summary> Gets the marginal carbon intensity value </summary>
    /// <param name="input"> input from JSON request converted to input object with location and time interval </param>
    /// <returns>Result of the call to the aggregator to retrieve carbon intenstiy</returns>
    public async Task<IActionResult> GetCarbonIntensityAsync(SciScoreInput input)
    {
        using (var activity = _activitySource.StartActivity(nameof(SciScoreController)))
        {
            _logger.LogDebug("calling to aggregator to calculate the average carbon intensity with input: {input}", input);

            var carbonIntensity = await _aggregator.CalculateAverageCarbonIntensityAsync(GetLocation(input.Location), input.TimeInterval);

            SciScore score = new SciScore
            {
                MarginalCarbonIntensityValue = carbonIntensity,
            };
            _logger.LogDebug("calculated marginal carbon intensity: {score}", score);
            return Ok(score);
        }
    }

    /// Validate the user input location and convert it to the internal Location object.
    //  Throws ArgumentException if input is invalid.
    private Location GetLocation(LocationInput locationInput)
    {
        LocationType locationType;
        CloudProvider cloudProvider;

        if (!Enum.TryParse<LocationType>(locationInput.LocationType, true, out locationType))
        {
            _logger.LogError("Can't parse location type with location input: ", locationInput);
            throw new ArgumentException($"locationType '{locationInput.LocationType}' is invalid");
        }

        Enum.TryParse<CloudProvider>(locationInput.CloudProvider, true, out cloudProvider);
        var location = new Location
        {
            LocationType = locationType,
            Latitude = locationInput.Latitude,
            Longitude = locationInput.Longitude,
            CloudProvider = cloudProvider,
            RegionName = locationInput.RegionName
        };

        return location;
    }


}