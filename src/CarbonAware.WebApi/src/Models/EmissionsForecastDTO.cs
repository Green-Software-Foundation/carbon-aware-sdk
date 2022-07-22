namespace CarbonAware.WebApi.Models;

using CarbonAware.Model;
using System.Text.Json.Serialization;

[Serializable]
public record EmissionsForecastDTO : EmissionsForecastBaseDTO
{
    /// <summary>
    /// Timestamp when the forecast was generated.
    /// </summary>
    /// <example>2022-06-01T00:00:00Z</example>
    [JsonPropertyName("generatedAt")]
    public DateTimeOffset GeneratedAt { get; set; }

    /// <summary>
    /// The optimal forecasted data point within the 'forecastData' array.
    /// Null if 'forecastData' array is empty.
    /// </summary>
    /// <example>
    /// {
    ///   "location": "eastus",
    ///   "timestamp": "2022-06-01T14:45:00Z",
    ///   "duration": 30,
    ///   "value": 359.23
    /// }
    /// </example>
    [JsonPropertyName("optimalDataPoint")]
    public EmissionsDataDTO? OptimalDataPoint { get; set; }

    /// <summary>
    /// The forecasted data points transformed and filtered to reflect the specified time and window parameters.
    /// Points are ordered chronologically; Empty array if all data points were filtered out.
    /// E.G. dataStartAt and dataEndAt times outside the forecast period; windowSize greater than total duration of forecast data;
    /// </summary>
    /// <example>
    /// [
    ///   {
    ///     "location": "eastus",
    ///     "timestamp": "2022-06-01T14:40:00Z",
    ///     "duration": 30,
    ///     "value": 380.99
    ///   },
    ///   {
    ///     "location": "eastus",
    ///     "timestamp": "2022-06-01T14:45:00Z",
    ///     "duration": 30,
    ///     "value": 359.23
    ///   },
    ///   {
    ///     "location": "eastus",
    ///     "timestamp": "2022-06-01T14:50:00Z",
    ///     "duration": 30,
    ///     "value": 375.12
    ///   }
    /// ]
    /// </example>
    [JsonPropertyName("forecastData")]
    public IEnumerable<EmissionsDataDTO>? ForecastData { get; set; }

    public static EmissionsForecastDTO FromEmissionsForecast(EmissionsForecast emissionsForecast)
    {
        return new EmissionsForecastDTO
        {
            GeneratedAt = emissionsForecast.GeneratedAt,
            Location = emissionsForecast.Location.DisplayName,
            DataStartAt = emissionsForecast.DataStartAt,
            DataEndAt = emissionsForecast.DataEndAt,
            WindowSize = (int)emissionsForecast.WindowSize.TotalMinutes,
            OptimalDataPoint = EmissionsDataDTO.FromEmissionsData(emissionsForecast.OptimalDataPoint),
            ForecastData = emissionsForecast.ForecastData.Select(d => EmissionsDataDTO.FromEmissionsData(d))!
        };
    }
}