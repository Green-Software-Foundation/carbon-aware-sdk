using System.Text.Json.Serialization;

namespace CarbonAware.DataSources.ElectricityMapsFree.Model;

/// <summary>
/// An object describing the emissions for a given countryCode
/// </summary>
[Serializable]
public record GridEmissionDataPoint
{

    [JsonPropertyName("_disclaimer")]
    public string Disclaimer { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("countryCode")]
    public string CountryCodeAbbreviation { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public Data Data { get; set; } = new Data();

    [JsonPropertyName("units")]
    public Units Units { get; set; } = new Units();

}

public record Data
{
    [JsonPropertyName("datetime")]
    public DateTimeOffset Datetime { get; set; }

    [JsonPropertyName("carbonIntensity")]
    public float CarbonIntensity { get; set; }

    [JsonPropertyName("fossilFuelPercentage")]
    public float FossilFuelPercentage { get; set; }
}

public record Units
{
    [JsonPropertyName("carbonIntensity")]
    public string CarbonIntensity { get; set; } = string.Empty;
}