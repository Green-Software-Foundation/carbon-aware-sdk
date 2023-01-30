using GSF.CarbonAware.Handlers.CarbonAware;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CarbonAware.WebApi.Models;

public class EmissionsDataForLocationsParametersDTO : CarbonAwareParametersBaseDTO
{
    /// <summary>String array of named locations</summary>
    /// <example>eastus</example>
    [FromQuery(Name = "location"), SwaggerParameter(Required = true)]
    public override string[]? MultipleLocations { get; set; }

    /// <summary>[Optional] Start time for the data query.</summary>
    /// <example>2022-03-01T15:30:00Z</example>
    [FromQuery(Name = "time")]
    public override DateTimeOffset? Start { get; set; }

    /// <summary>[Optional] End time for the data query.</summary>
    /// <example>2022-03-01T18:30:00Z</example>
    [FromQuery(Name = "toTime")]
    public override DateTimeOffset? End { get; set; }
}