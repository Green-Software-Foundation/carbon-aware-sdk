using CarbonAware.Model;
using System.Text.Json.Serialization;

namespace CarbonAware.DataSources.ElectricityMaps.Model;

/// <summary>
/// A collection of forecast carbon intensity instances.
/// </summary>
[Serializable]
public record ForecastedCarbonIntensityData
{
    /// <summary>
    /// Zone.
    /// </summary>
    [JsonPropertyName("zone")]
    public string Zone { get; init; } = string.Empty;

    /// <summary>
    /// A list of Forecast instances.
    /// </summary>
    [JsonPropertyName("forecast")]
    public IEnumerable<Forecast> ForecastData { get; init; } = Array.Empty<Forecast>();

    /// <summary>
    /// DateTime indicating when the forecast carbon intensity was updated.
    /// </summary>
    [JsonPropertyName("updatedAt")]
    public DateTimeOffset UpdatedAt { get; init; }

    public static explicit operator EmissionsForecast(ForecastedCarbonIntensityData electricityMapsForecast)
    {
        return new EmissionsForecast
        {
            ForecastData = electricityMapsForecast.ForecastData.Select(d => (EmissionsData) d),
            GeneratedAt = electricityMapsForecast.UpdatedAt,         
        };
    }
}

/// <summary>
/// A forecast for a given time period.
/// </summary>
[Serializable]
public record Forecast
{
    /// <summary>
    /// Carbon Intensity value
    /// </summary>
    [JsonPropertyName("carbonIntensity")]
    public int CarbonIntensity { get; init; }

    /// <summary>
    /// DateTime indicating when the carbon intensity was generated.
    /// </summary>
    [JsonPropertyName("datetime")]
    public DateTimeOffset DateTime { get; init; }

    public static explicit operator EmissionsData(Forecast electricityMapsForecast)
    {
        return new EmissionsData()
        {
            Rating = electricityMapsForecast.CarbonIntensity,
            Time = electricityMapsForecast.DateTime,
        };
    }
}
