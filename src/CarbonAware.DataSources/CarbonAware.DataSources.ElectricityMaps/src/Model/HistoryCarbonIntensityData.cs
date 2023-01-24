using CarbonAware.Exceptions;
using CarbonAware.Model;
using System.Text.Json.Serialization;

namespace CarbonAware.DataSources.ElectricityMaps.Model;

/// <summary>
/// History Carbon Intensity collection data.
/// </summary>
[Serializable]
public record HistoryCarbonIntensityData
{
    /// <summary>
    /// Zone.
    /// </summary>
    [JsonPropertyName("zone")]
    public string Zone { get; init; } = string.Empty;

    /// <summary>
    /// List of History Carbon Intensity instances.
    /// </summary>
    [JsonPropertyName("history")]
    public IEnumerable<CarbonIntensity> HistoryData { get; init; } = Array.Empty<CarbonIntensity>();
}

/// <summary>
/// A history carbon intensity.
/// </summary>
[Serializable]
public record CarbonIntensity
{
    /// <summary>
    /// Carbon Intensity value.
    /// </summary>
    [JsonPropertyName("carbonIntensity")]
    public int Value { get; init; }

    /// <summary>
    /// Indicates the datetime of the carbon intensity
    /// </summary>
    [JsonPropertyName("datetime")]
    public DateTimeOffset DateTime { get; init; }

    /// <summary>
    /// Indicates the datetime the carbon intensity was updated
    /// </summary>
    [JsonPropertyName("updatedAt")]
    public DateTimeOffset UpdatedAt { get; init; }

    /// <summary>
    /// Indicates the datetime the carbon intensity was created at
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Indicated the emission factor type used for computing the carbon intensity.
    /// </summary>
    [JsonPropertyName("emissionFactorType")]
    public string? EmissionFactorType { get; init; }

    /// <summary>
    /// Indicates whether the result is estimated or no
    /// </summary>
    [JsonPropertyName("isEstimated")]
    public Boolean IsEstimated { get; init; }

    /// <summary>
    /// Estimation method.
    /// </summary>
    [JsonPropertyName("estimationMethod")]
    public string? EstimationMethod { get; init; }

    public static explicit operator EmissionsData(CarbonIntensity historyCarbonIntensity)
    {
        return new EmissionsData
        {
            Rating = historyCarbonIntensity.Value,
            Time = historyCarbonIntensity.UpdatedAt,
        };
    }
}

/// <summary>
/// Carbon intensity data for past date range.
/// </summary>
[Serializable]
public record PastRangeData
{
    /// <summary>
    /// Carbon Intensity value.
    /// </summary>
    [JsonPropertyName("data")]
    public IEnumerable<CarbonIntensity> HistoryData { get; init; } = Array.Empty<CarbonIntensity>();

}

