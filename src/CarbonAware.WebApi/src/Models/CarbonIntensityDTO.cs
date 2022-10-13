using System.Text.Json.Serialization;

namespace CarbonAware.WebApi.Models;

[Serializable]
public record CarbonIntensityDTO
{
    /// <summary>the location name where workflow is run </summary>
    /// <example>eastus</example>
    [JsonPropertyName("location")]
    public string? Location { get; set; } = string.Empty;

    /// <summary>the time at which the workflow we are measuring carbon intensity for started </summary>
    /// <example>2022-03-01T15:30:00Z</example>
    [JsonPropertyName("startTime")]
    public DateTimeOffset? StartTime { get; set; }

    /// <summary> the time at which the workflow we are measuring carbon intensity for ended</summary>
    /// <example>2022-03-01T18:30:00Z</example>
    [JsonPropertyName("endTime")]
    public DateTimeOffset? EndTime { get; set; }

    /// <summary>Value of the marginal carbon intensity in grams per kilowatt-hour.</summary>
    /// <example>345.434</example>
    [JsonPropertyName("carbonIntensity")]
    public double CarbonIntensity { get; set; }
}