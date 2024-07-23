using System.Text.Json.Serialization;

namespace CarbonAware.DataSources.WattTime.Model;

/// <summary>
/// Data type used to capture the model as part of an EmissionsDataResponse
/// </summary>
[Serializable]
internal record GridEmissionsModelData
{
    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; } = String.Empty;
}