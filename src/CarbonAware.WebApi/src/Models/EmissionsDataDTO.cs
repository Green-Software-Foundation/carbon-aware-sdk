namespace CarbonAware.WebApi.Models;

using System.Text.Json.Serialization;

[Serializable]
public record EmissionsDataDTO
{
    /// <example>eastus</example>
    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    /// <example>2022-06-01T14:45:00Z</example>
    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; }

    /// <example>30</example>
    [JsonPropertyName("duration")]
    public int Duration { get; set; }

    /// <example>359.23</example>
    [JsonPropertyName("value")]
    public double Value { get; set; }

    public static EmissionsDataDTO? FromEmissionsData(global::CarbonAware.Model.EmissionsData emissionsData)
    {
        if (emissionsData == null)
        {
            return null;
        }
        return new EmissionsDataDTO
        {
            Location = emissionsData.Location,
            Timestamp = emissionsData.Time,
            Duration = (int)emissionsData.Duration.TotalMinutes,
            Value = emissionsData.Rating
        };
    }

    public static EmissionsDataDTO? FromEmissionsData(global::GSF.CarbonAware.Models.EmissionsData emissionsData)
    {
        if (emissionsData == null)
        {
            return null;
        }
        return new EmissionsDataDTO
        {
            Location = emissionsData.Location!,
            Timestamp = emissionsData.Time,
            Duration = (int)emissionsData.Duration.TotalMinutes,
            Value = emissionsData.Rating
        };
    }
}