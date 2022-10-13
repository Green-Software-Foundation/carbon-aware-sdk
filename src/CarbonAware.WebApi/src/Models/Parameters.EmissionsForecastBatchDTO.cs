using CarbonAware.Aggregators.CarbonAware;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace CarbonAware.WebApi.Models;

public class EmissionsForecastBatchParametersDTO : CarbonAwareParametersBaseDTO
{
    /// <summary>
    /// For historical forecast requests, this value is the timestamp used to access the most
    /// recently generated forecast as of that time.
    /// </summary>
    /// <example>2022-06-01T00:03:30Z</example>
    [JsonPropertyName("requestedAt"), SwaggerParameter(Required = true)]
    public override DateTimeOffset? Requested { get; set; }

    /// <summary>The location of the forecast</summary>
    /// <example>eastus</example>
    [JsonPropertyName("location"), SwaggerParameter(Required = true)]
    public override string? SingleLocation { get; set; }

    /// <summary>
    /// Start time boundary of forecasted data points.Ignores current forecast data points before this time.
    /// Defaults to the earliest time in the forecast data.
    /// </summary>
    /// <example>2022-03-01T15:30:00Z</example>
    [JsonPropertyName("dataStartAt")]
    public override DateTimeOffset? Start { get; set; }

    /// <summary>
    /// End time boundary of forecasted data points. Ignores current forecast data points after this time.
    /// Defaults to the latest time in the forecast data.
    /// </summary>
    /// <example>2022-03-01T18:30:00Z</example>
    [JsonPropertyName("dataEndAt")]
    public override DateTimeOffset? End { get; set; }

    /// <summary>
    /// The estimated duration (in minutes) of the workload.
    /// Defaults to the duration of a single forecast data point.
    /// </summary>
    /// <example>30</example>
    [JsonPropertyName("windowSize")]
    public override int? Duration { get; set; }
}
