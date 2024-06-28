using System.Text.Json.Serialization;

namespace CarbonAware.DataSources.WattTime.Model;

[Serializable]
internal record GridEmissionsMetaData
{
    [JsonPropertyName("data_point_periods_second")]
    public int DataPointPeriodSeconds { get; set; }

    /// <summary>
    /// Region (abbreviation)
    /// </summary>
    [JsonPropertyName("region")]
    public string Region { get; set; } = string.Empty;

    /// <summary>
    /// Signal Type. eg MOER
    /// </summary>
    [JsonPropertyName("signal_type")]
    public string? SignalType { get; set; }

    [JsonPropertyName("model")]
    public GridEmissionsModelData? Model { get; set; }

    [JsonPropertyName("units")]
    public string? Units { get; set; }

    [JsonPropertyName("generated_at_period_seconds")]
    public int? GeneratedAtPeriodSeconds { get; set; }

    [JsonPropertyName("generated_at")]
    public DateTimeOffset GeneratedAt { get; set; } = DateTimeOffset.MinValue;
}