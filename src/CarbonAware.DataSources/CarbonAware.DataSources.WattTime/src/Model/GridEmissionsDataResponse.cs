using System.Text.Json.Serialization;

namespace CarbonAware.DataSources.WattTime.Model;

[Serializable]
internal record GridEmissionsDataResponse
{
    [JsonPropertyName("data")]
    public List<GridEmissionDataPoint> Data { get; set; } = new List<GridEmissionDataPoint>();


    [JsonPropertyName("meta")]
    public GridEmissionsMetaData Meta { get; set; }
}


