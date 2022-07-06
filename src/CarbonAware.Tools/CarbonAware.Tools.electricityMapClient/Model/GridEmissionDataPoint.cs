using System.Text.Json.Serialization;

namespace CarbonAware.Tools.electricityMapClient.Model;

/// <summary>
/// An object describing the emissions for a given time period and balancing authority.
/// </summary>
[Serializable]
public record GridEmissionDataPoint
{
    /// <summary>
    /// Balancing authority abbreviation
    /// </summary>
    [JsonPropertyName("countryCode")]
    public string countryCodeAbbreviation { get; set; } = string.Empty;

    /// <summary>
    /// Market type, only useful for grid data other than MOERs.
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    /// <summary>
    /// TODO: need deserialize or access to a property in a nested object(data) e.g. make another class
    /// </summary>
    [JsonPropertyName("data")]
    public string[] data { get; set; }

}
