namespace CarbonAware.WebApi.Models;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

[Serializable]
public record EmissionsForecastBaseDTO
{
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
    [JsonPropertyName("startTime")]
    public DateTimeOffset StartTime { get; set; }

    /// <summary>
    /// End time boundary of forecasted data points. Ignores forecast data points after this time.
    /// Defaults to the latest time in the forecast data.
    /// </summary>
    /// <example>2022-06-01T18:00:00Z</example>
    [JsonPropertyName("endTime")]
    public DateTimeOffset EndTime { get; set; }

    /// <summary>
    /// The estimated duration (in minutes) of the workload.
    /// Defaults to the duration of a single forecast data point.
    /// </summary>
    /// <example>30</example>
    [JsonPropertyName("windowSize")]
    public int WindowSize { get; set; }
}