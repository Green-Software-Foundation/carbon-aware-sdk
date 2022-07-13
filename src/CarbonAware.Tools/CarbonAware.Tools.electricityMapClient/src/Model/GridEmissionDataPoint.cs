using System.Text.Json.Serialization;

namespace CarbonAware.Tools.electricityMapClient.Model;

/// <summary>
/// An object describing the emissions for a given countryCode
/// </summary>
[Serializable]
public record GridEmissionDataPoint
{
    [JsonPropertyName("countryCode")]
    public string countryCodeAbbreviation { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    /// <summary>
    /// TODO: need deserialize or access to a property in a nested object(data)
    /// </summary>
    [JsonPropertyName("data")]
    public string[]? data { get; set; }

}
