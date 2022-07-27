using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CarbonAware.WebApi.Models;

[Serializable]
public record EmissionsForecastBatchDTO : EmissionsForecastBaseDTO
{
    /// <summary>
    /// For historical forecast requests, this value is the timestamp used to access the most
    /// recently generated forecast as of that time.
    /// </summary>
    /// <example>2022-06-01T00:03:30Z</example>
    [JsonPropertyName("requestedAt")]
    [Required]
    public new DateTimeOffset? RequestedAt { get; set; }

    /// <summary>The location of the forecast</summary>
    /// <example>eastus</example>
    [JsonPropertyName("location")]
    [Required]
    public new string? Location { get; set; }
}
