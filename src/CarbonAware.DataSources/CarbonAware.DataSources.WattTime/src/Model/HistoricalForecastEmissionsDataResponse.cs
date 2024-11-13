using System.Text.Json.Serialization;

namespace CarbonAware.DataSources.WattTime.Model;

[Serializable]
internal record HistoricalForecastEmissionsDataResponse
{
    [JsonPropertyName("data")]
    public List<HistoricalEmissionsData> Data { get; set; } = new List<HistoricalEmissionsData>();


    [JsonPropertyName("meta")]
    public GridEmissionsMetaData Meta { get; set; } = new GridEmissionsMetaData();
}


