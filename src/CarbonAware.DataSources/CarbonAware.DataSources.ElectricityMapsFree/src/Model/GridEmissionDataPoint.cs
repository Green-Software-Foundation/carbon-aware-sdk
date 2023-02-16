using System.Text.Json.Serialization;

namespace CarbonAware.DataSources.ElectricityMapsFree.Model;

/// <summary>
/// An object describing the emissions for a given countryCode
/// </summary>
[Serializable]
public record GridEmissionDataPoint
{

    [JsonPropertyName("_disclaimer")]
    public string? Disclaimer { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("countryCode")]
    public string CountryCodeAbbreviation { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public Data? Data { get; set; }

    [JsonPropertyName("units")]
    public Units? Units { get; set; }

}

public record Data
{
    public DateTimeOffset Datetime { get; set; }
    public float CarbonIntensity { get; set; }
    public float FossilFuelPercentage { get; set; }
}

public record Units
{
    public string? CarbonIntensity { get; set; }
}