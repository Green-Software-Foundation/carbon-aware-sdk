using CarbonAware.Model;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace CarbonAware.WebApi.Controllers;

[ApiController]
[Microsoft.AspNetCore.Mvc.Route("sci-scores")]
public class SciScoreController : ControllerBase
{
    private readonly ILogger<SciScoreController> _logger;

    public SciScoreController(ILogger<SciScoreController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsync(SciScoreCalculation calculation)
    {
        if (String.IsNullOrEmpty(calculation.AzRegion))
        {
            return BadRequest("AzRegion is required");
        }

        if (String.IsNullOrEmpty(calculation.Duration))
        {
            return BadRequest("Duration is required");
        }

        SciScore score = new SciScore
            {
                SciScoreValue = 100.0f,
                EnergyValue = 1.0f,
                MarginalCarbonEmissionsValue = 100.0f,
                EmbodiedEmissionsValue = 0.0f,
                FunctionalUnitValue = 1
            };

        return await Task.Run(() => Ok(score));
    }
}
