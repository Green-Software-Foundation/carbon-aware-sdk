using CarbonAware.Model;
using System.Text.Json.Serialization;

namespace CarbonAware.Tools.electricityMapClient.Model;

/// <summary>
/// The details of the balancing authority (BA) serving a particular location.
/// </summary>
[Serializable]
public record Zone
{
    /// <summary>
    /// Balancing authority abbreviation.
    /// </summary>
    [JsonPropertyName("countryName")]
    public string countryName { get; set; } = string.Empty;

    /// <summary>
    /// Unique WattTime id for the region.
    /// </summary>
    [JsonPropertyName("countryCode")]
    public string countryCode { get; set; } = string.Empty;

    /// <summary>
    /// Human readable name/description for the region.
    /// </summary>
    [JsonPropertyName("zoneName")]
    public string zoneName { get; set; } = string.Empty;

    public static implicit operator Zone(Location v)
    {
        throw new NotImplementedException();
    }
}
