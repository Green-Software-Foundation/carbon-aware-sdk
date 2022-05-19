using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;

namespace csharp.Controllers;

[ApiController]
[Route("emissions")]
public class CarbonAwareApiController : ControllerBase
{
    private readonly ILogger<CarbonAwareApiController> logger;
    private readonly IConfig config;

    public CarbonAwareApiController(ILogger<CarbonAwareApiController> logger, IConfig config)
    {
        this.logger = logger;
        this.config = config;
    }

    [HttpGet("byLocation")]
    public async Task<IActionResult> Get(
        [FromQuery] string location,
        [FromQuery] string time,
        [FromQuery] string toTime,
        [FromQuery] int durationMinutes
    )
    {
        var config = new Configuration();
        config.BasePath = this.config.BASE_URL;
        var caa = new CarbonAwareApi(config);
        var payload = await caa.EmissionsBylocationGetAsync(
            location: location,
            time: string.IsNullOrEmpty(time) ? null : DateTime.Parse(time),
            toTime: string.IsNullOrEmpty(toTime) ? null : DateTime.Parse(toTime),
            durationMinutes: durationMinutes
        );
        return Ok(payload);
    }
}
