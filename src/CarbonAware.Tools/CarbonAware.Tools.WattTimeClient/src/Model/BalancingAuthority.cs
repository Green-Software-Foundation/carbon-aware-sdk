using System.Text.Json.Serialization;

namespace CarbonAware.Tools.WattTimeClient.Model;

/// <summary>
/// The details of the balancing authority (BA) serving a particular location.
/// </summary>
[Serializable]
public record BalancingAuthority
{
    /// <summary>
    /// Balancing authority abbreviation.
    /// </summary>
    [JsonPropertyName("abbrev")]
    public string Abbreviation { get; set; } = string.Empty;

    /// <summary>
    /// Unique WattTime id for the region.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// Human readable name/description for the region.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

}
