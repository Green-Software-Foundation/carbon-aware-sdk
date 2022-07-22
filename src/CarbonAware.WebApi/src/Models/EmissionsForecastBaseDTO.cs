namespace CarbonAware.WebApi.Models;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

[Serializable]
public record EmissionsForecastBaseDTO
{
    /// <summary>
    /// For current requests, this value is the timestamp the request for forecast data was made.
    /// For historical forecast requests, this value is the timestamp used to access the most 
    /// recently generated forecast as of that time. 
    /// </summary>
    /// <example>2022-06-01T00:03:30Z</example>
    [JsonPropertyName("requestedAt")]
    [Required()]
    public DateTimeOffset RequestedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>The location of the forecast</summary>
    /// <example>eastus</example>
    [JsonPropertyName("location")]
    [Required()]
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Start time boundary of forecasted data points. Ignores forecast data points before this time.
    /// Defaults to the earliest time in the forecast data.
    /// </summary>
    /// <example>2022-06-01T12:00:00Z</example>
    [JsonPropertyName("dataStartAt")]
    public DateTimeOffset DataStartAt { get; set; }

    /// <summary>
    /// End time boundary of forecasted data points. Ignores forecast data points after this time.
    /// Defaults to the latest time in the forecast data.
    /// </summary>
    /// <example>2022-06-01T18:00:00Z</example>
    [JsonPropertyName("dataEndAt")]
    public DateTimeOffset DataEndAt { get; set; }

    /// <summary>
    /// The estimated duration (in minutes) of the workload.
    /// Defaults to the duration of a single forecast data point.
    /// </summary>
    /// <example>30</example>
    [JsonPropertyName("windowSize")]
    public int WindowSize { get; set; }
}