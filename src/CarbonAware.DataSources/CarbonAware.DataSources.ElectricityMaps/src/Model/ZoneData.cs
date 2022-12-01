using System.Text.Json.Serialization;

namespace CarbonAware.DataSources.ElectricityMaps.Model;

/// <summary>
/// Object containing the zones and routes accessible with the token.
/// </summary>
[Serializable]
internal record ZoneData
{
    [JsonPropertyName("zoneName")]
    public string ZoneName { get; init; } = string.Empty;

    [JsonPropertyName("access")]
    public string[] Access { get; init; } = Array.Empty<string>();
}
