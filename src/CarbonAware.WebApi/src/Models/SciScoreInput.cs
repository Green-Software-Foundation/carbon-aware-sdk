using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CarbonAware.WebApi.Models;

[Serializable]
public record SciScoreInput
{
    [JsonPropertyName("location")]
    [Required()]
    public LocationInput Location { get; set; } = new LocationInput();

    [JsonPropertyName("timeInterval")]
    [Required()]
    public string TimeInterval { get; set; } = string.Empty;
}

[Serializable]
public record LocationInput
{
    [JsonPropertyName("locationType")]
    [Required()]
    public string? LocationType { get; set; }

    [JsonPropertyName("latitude")]
    public decimal? Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public decimal? Longitude { get; set; }

    [JsonPropertyName("cloudProvider")]
    public string? CloudProvider { get; set; }

    [JsonPropertyName("regionName")]
    public string? RegionName { get; set; }
}