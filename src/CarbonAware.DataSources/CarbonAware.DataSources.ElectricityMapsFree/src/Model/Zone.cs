using System.Text.Json.Serialization;

namespace CarbonAware.DataSources.ElectricityMapsFree.Model;

/// <summary>
/// The details of the zone serving a particular location.
/// </summary>
[Serializable]
public record Zone
{
    /// <summary>
    /// Unique countryCode for the region. such as AUS-NSW
    /// </summary>
    [JsonPropertyName("countryCode")]
    public string countryCode { get; set; } = string.Empty;

    /// <summary>
    /// CountryName such as Australia
    /// </summary>
    [JsonPropertyName("countryName")]
    public string? countryName { get; set; } = string.Empty;

    /// <summary>
    /// ZoneName such as New South Wales
    /// </summary>
    [JsonPropertyName("zoneName")]
    public string zoneName { get; set; } = string.Empty;

}
