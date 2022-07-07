namespace CarbonAware.Model;

public record EmissionsForecast
{
    /// <summary>
    /// Gets or sets the time that the forecast was generated.
    /// </summary>
    public DateTimeOffset GeneratedAt { get; set; }

    /// <summary>
    /// Gets or sets the location the forecast is for.
    /// </summary>
    public Location Location { get; set; } = new Location(){ LocationType = LocationType.NotProvided };
   
    /// <summary>
    /// Gets or sets the start time of the forecast data points.
    /// </summary>
    public DateTimeOffset StartTime { get; set; }

    /// <summary>
    /// Gets or sets the end time of the forecast data points.
    /// </summary>
    public DateTimeOffset EndTime { get; set; }

    /// <summary>
    /// Gets or sets rolling average window duration.
    /// </summary>
    public TimeSpan WindowSize { get; set; }

    /// <summary>
    /// Gets or sets the forecast data points.
    /// </summary>
    public IEnumerable<EmissionsData> ForecastData { get; set; } = new List<EmissionsData>();

    /// <summary>
    /// Gets or sets the optimal data point within the ForecastData set.
    /// </summary>
    public EmissionsData OptimalDataPoint { get; set; }
}
