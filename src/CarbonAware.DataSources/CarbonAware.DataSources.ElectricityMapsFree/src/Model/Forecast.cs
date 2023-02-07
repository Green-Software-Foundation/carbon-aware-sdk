using System.Text.Json.Serialization;

namespace CarbonAware.DataSources.ElectricityMapsFree.Model;

/// <summary>
/// An emissions forecast for a given time period.
/// </summary>
[Serializable]
public record Forecast
{
    /// <summary>
    /// DateTime indicating when the forecast was generated.
    /// </summary>
    [JsonPropertyName("generated_at")]
    public DateTimeOffset GeneratedAt { get; set; }

    /// <summary>
    /// List of GridEmissionDataPoints representing the predicted values for those points in time.
    /// </summary>
    [JsonPropertyName("forecast")]
    public List<GridEmissionDataPoint> ForecastData { get; set; } = new List<GridEmissionDataPoint>();
}
