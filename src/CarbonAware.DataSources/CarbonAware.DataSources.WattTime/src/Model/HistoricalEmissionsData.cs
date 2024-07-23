using System.Text.Json.Serialization;

namespace CarbonAware.DataSources.WattTime.Model;

[Serializable]
internal class HistoricalEmissionsData
{
    [JsonPropertyName("generated_at")]
    public DateTimeOffset GeneratedAt { get; set; } = DateTimeOffset.MinValue;

    [JsonPropertyName("forecast")]
    public List<GridEmissionDataPoint> Forecast { get; set; } = new List<GridEmissionDataPoint>();

}

