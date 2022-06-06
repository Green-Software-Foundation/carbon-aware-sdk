using System.Text.Json.Serialization;

namespace CarbonAware.Tools.WattTimeClient.Model;

/// <summary>
/// An object describing the emissions for a given time period and balancing authority.
/// </summary>
[Serializable]
public record GridEmissionDataPoint
{
    /// <summary>
    /// Balancing authority abbreviation
    /// </summary>
    [JsonPropertyName("ba")]
    public string BalancingAuthorityAbbreviation { get; set; } = string.Empty;

    /// <summary>
    /// Type of data. eg MOER
    /// </summary>
    [JsonPropertyName("datatype")]
    public string? Datatype { get; set; }

    /// <summary>
    /// Duration in seconds for which the data is valid from point_time.
    /// </summary>
    [JsonPropertyName("frequency")]
    public int? Frequency { get; set; }

    /// <summary>
    /// Market type, only useful for grid data other than MOERs.
    /// </summary>
    [JsonPropertyName("market")]
    public string? Market { get; set; }

    /// <summary>
    /// DateTime indicating when this data became valid.
    /// </summary>
    [JsonPropertyName("point_time")]
    public DateTimeOffset PointTime { get; set; }

    /// <summary>
    /// Number value of data (corresponding to datatype).
    /// </summary>
    [JsonPropertyName("value")]
    public float Value { get; set; }

    /// <summary>
    /// MOER version (Not present and not applicable for other datatypes)
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;
}
