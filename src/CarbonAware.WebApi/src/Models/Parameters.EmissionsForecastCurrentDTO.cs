using CarbonAware.Aggregators.CarbonAware;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CarbonAware.WebApi.Models;

public class EmissionsForecastCurrentParametersDTO : CarbonAwareParametersBaseDTO
{
    /// <summary>String array of named locations</summary>
    /// <example>eastus</example>
    [FromQuery(Name = "location"), SwaggerParameter(Required = true)]
    public override string[]? MultipleLocations { get; set; }

    /// <summary>
    /// Start time boundary of forecasted data points.Ignores current forecast data points before this time.
    /// Defaults to the earliest time in the forecast data.
    /// </summary>
    /// <example>2022-03-01T15:30:00Z</example>
    [FromQuery(Name = "dataStartAt")]
    public override DateTimeOffset? Start { get; set; }

    /// <summary>
    /// End time boundary of forecasted data points. Ignores current forecast data points after this time.
    /// Defaults to the latest time in the forecast data.
    /// </summary>
    /// <example>2022-03-01T18:30:00Z</example>
    [FromQuery(Name = "dataEndAt")]
    public override DateTimeOffset? End { get; set; }

    /// <summary>
    /// The estimated duration (in minutes) of the workload.
    /// Defaults to the duration of a single forecast data point.
    /// </summary>
    /// <example>30</example>
    [FromQuery(Name = "windowSize")]
    public override int? Duration { get; set; }
}