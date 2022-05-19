using System.Text.Json.Serialization;

namespace CarbonAware.WebApi.Models;

[Serializable]
public record SciScoreInput
{
    [JsonPropertyName("location")]
    public LocationInput? Location { get; set; }

    [JsonPropertyName("timeInterval")]
    public string TimeInterval { get; set; } = string.Empty;
}

[Serializable]
public record LocationInput
{
    [JsonPropertyName("locationType")]
    public string LocationType { get; set; } = string.Empty;

    [JsonPropertyName("latitude")]
    public decimal? Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public decimal? Longitude { get; set; }

    [JsonPropertyName("cloudProvider")]
    public string? CloudProvider { get; set; }

    [JsonPropertyName("regionName")]
    public string? RegionName { get; set; }
}