using CarbonAware.DataSources.WattTime.Constants;
using System.Text.Json.Serialization;

namespace CarbonAware.DataSources.WattTime.Model;

/// <summary>
/// The details of the region serving a particular location.
/// </summary>
[Serializable]
internal record RegionResponse
{
    /// <summary>
    /// Region abbreviation.
    /// </summary>
    [JsonPropertyName("region")]
    public string Region { get; set; } = string.Empty;

    /// <summary>
    /// Signal Type
    /// </summary>
    [JsonPropertyName("signal_type")]
    public SignalTypes SignalType { get; set; }

    /// <summary>
    /// Human readable name/description for the region.
    /// </summary>
    [JsonPropertyName("region_full_name")]
    public string RegionFullName { get; set; } = string.Empty;

}
