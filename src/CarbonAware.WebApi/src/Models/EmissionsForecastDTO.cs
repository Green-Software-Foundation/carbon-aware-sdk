namespace CarbonAware.WebApi.Models;

using CarbonAware.Model;
using System.Text.Json.Serialization;

[Serializable]
public record EmissionsForecastDTO
{
    /// <summary>
    /// Timestamp when the forecast was generated.
    /// </summary>
    /// <example>2022-06-01T00:00:00Z</example>
    [JsonPropertyName("generatedAt")]
    public DateTimeOffset GeneratedAt { get; set; }

    /// <summary>
    /// For current requests, this value is the timestamp the request for forecast data was made.
    /// For historical forecast requests, this value is the timestamp used to access the most 
    /// recently generated forecast as of that time. 
    /// </summary>
    /// <example>2022-06-01T00:03:30Z</example>
    [JsonPropertyName("requestedAt")]
    public DateTimeOffset RequestedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>The location of the forecast</summary>
    /// <example>eastus</example>
    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Start time boundary of forecasted data points. Ignores forecast data points before this time.
    /// Defaults to the earliest time in the forecast data.
    /// </summary>
    /// <example>2022-06-01T12:00:00Z</example>
    [JsonPropertyName("dataStartAt")]
    public DateTimeOffset DataStartAt { get; set; }

    /// <summary>
    /// End time boundary of forecasted data points. Ignores forecast data points after this time.
    /// Defaults to the latest time in the forecast data.
    /// </summary>
    /// <example>2022-06-01T18:00:00Z</example>
    [JsonPropertyName("dataEndAt")]
    public DateTimeOffset DataEndAt { get; set; }

    /// <summary>
    /// The estimated duration (in minutes) of the workload.
    /// Defaults to the duration of a single forecast data point.
    /// </summary>
    /// <example>30</example>
    [JsonPropertyName("windowSize")]
    public int WindowSize { get; set; }

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
            ForecastData = emissionsForecast.ForecastData.Select(d => EmissionsDataDTO.FromEmissionsData(d))!,
            RequestedAt = emissionsForecast.RequestedAt
        };
    }
}