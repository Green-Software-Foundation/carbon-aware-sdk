namespace CarbonAware.Model;

public record EmissionsForecast
{
    /// <summary>
    /// Gets the time when the request was made
    /// </summary>
    public DateTimeOffset RequestedAt { get; set; }
    /// <summary>
    /// Gets or sets the time that the forecast was generated.
    /// </summary>
    public DateTimeOffset GeneratedAt { get; set; }

    /// <summary>
    /// Gets or sets the location the forecast is for.
    /// </summary>
    public Location Location { get; set; } = new();
   
    /// <summary>
    /// Gets or sets the start time of the forecast data points.
    /// </summary>
    public DateTimeOffset DataStartAt { get; set; }

    /// <summary>
    /// Gets or sets the end time of the forecast data points.
    /// </summary>
    public DateTimeOffset DataEndAt { get; set; }

    /// <summary>
    /// Gets or sets rolling average window duration.
    /// </summary>
    public TimeSpan WindowSize { get; set; }

    /// <summary>
    /// Gets or sets the forecast data points.
    /// </summary>
    public IEnumerable<EmissionsData> ForecastData { get; set; } = new List<EmissionsData>();

    /// <summary>
    /// Gets or sets the optimal data points within the ForecastData set.
    /// </summary>
    public IEnumerable<EmissionsData> OptimalDataPoints { get; set; }


    public void Validate()
    {
        var errors = new Dictionary<string, List<string>>();
        var firstDataPoint = ForecastData.First();
        var lastDataPoint = ForecastData.Last();
        var minTime = firstDataPoint.Time;
        var maxTime = lastDataPoint.Time + lastDataPoint.Duration;

        if (DataStartAt >= DataEndAt)
        {
            AddErrorMessage(errors, "dataStartAt", "dataStartAt must be earlier than dataEndAt");
        }

        if (DataStartAt < minTime || DataStartAt > maxTime)
        {
            AddErrorMessage(errors, "dataStartAt", $"dataStartAt must be within time range of the forecasted data, '{minTime}' through '{maxTime}'");
        }

        if (DataEndAt < minTime || DataEndAt > maxTime)
        {
            AddErrorMessage(errors, "dataEndAt", $"dataEndAt must be within time range of the forecasted data, '{minTime}' through '{maxTime}'");
        }

        if (errors.Keys.Count > 0)
        {
            ArgumentException error = new ArgumentException("Invalid EmissionsForecast");
            foreach (KeyValuePair<string, List<string>> message in errors)
            {
                error.Data[message.Key] = message.Value.ToArray();
            }
            throw error;
        }
    }

    private void AddErrorMessage(Dictionary<string, List<string>> errors, string key, string message)
    {
        if (!errors.ContainsKey(key))
        {
            errors[key] = new List<string>();
        }
        errors[key].Add(message);
    }
}
