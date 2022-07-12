namespace CarbonAware.WebApi.Models;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

[Serializable]
public record EmissionsForecastBatchDTO : EmissionsForecastBaseDTO
{
  /// <summary>The historical time used to fetch the most recent forecast as of that time.</summary>
  /// <example>2022-06-01T00:03:30Z</example>
  [JsonPropertyName("requestedAt")]
  [Required()]
  public DateTimeOffset RequestedAt { get; set; }
}