using System.Text.Json.Serialization;

namespace CarbonAware.WebApi.Models;

[Serializable]
public record SciScore
{
    [JsonPropertyName("sciScore")]
    public double? SciScoreValue { get; set; }

    [JsonPropertyName("energyValue")]
    public double? EnergyValue { get; set; }

    [JsonPropertyName("marginalCarbonIntensityValue")]
    public double? MarginalCarbonIntensityValue { get; set; }

    [JsonPropertyName("embodiedEmissionsValue")]
    public double? EmbodiedEmissionsValue { get; set; }

    [JsonPropertyName("functionalUnitValue")]
    public Int64? FunctionalUnitValue { get; set; }
}
