using CarbonAware.Aggregators.CarbonAware;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CarbonAware.WebApi.Models;

public class CarbonIntensityParametersDTO : CarbonAwareParametersBaseDTO
{
    /// <summary>The location name where workflow is run </summary>
    /// <example>eastus</example>
    [FromQuery(Name = "location"), SwaggerParameter(Required = true)]
    public override string? SingleLocation { get; set; }

    /// <summary>The time at which the workflow we are measuring carbon intensity for started </summary>
    /// <example>2022-03-01T15:30:00Z</example>
    [FromQuery(Name = "startTime"), SwaggerParameter(Required = true)]
    public override DateTimeOffset? Start { get; set; }

    /// <summary>The time at which the workflow we are measuring carbon intensity for ended</summary>
    /// <example>2022-03-01T18:30:00Z</example>
    [FromQuery(Name = "endTime"), SwaggerParameter(Required = true)]
    public override DateTimeOffset? End { get; set; }
}