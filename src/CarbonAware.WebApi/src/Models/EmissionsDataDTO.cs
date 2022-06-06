namespace CarbonAware.WebApi.Models;

using CarbonAware.Model;
using System.Text.Json.Serialization;

[Serializable]
public record EmissionsDataDTO
{
    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; }

    [JsonPropertyName("duration")]
    public int Duration { get; set; }

    [JsonPropertyName("value")]
    public double Value { get; set; }

    public static EmissionsDataDTO FromEmissionsData(EmissionsData emissionsData)
    {
        return new EmissionsDataDTO
        {
            Location = emissionsData.Location,
            Timestamp = emissionsData.Time,
            Duration = (int)emissionsData.Duration.TotalMinutes,
            Value = emissionsData.Rating
        };
    }
}