namespace CarbonAware.Model;

using System.ComponentModel.DataAnnotations;

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


    public void Validate()
    {
        var errors = new Dictionary<string, List<string>>();
        var firstDataPoint = ForecastData.First();
        var lastDataPoint = ForecastData.Last();
        var minTime = firstDataPoint.Time;
        var maxTime = lastDataPoint.Time + lastDataPoint.Duration;

        if (StartTime >= EndTime)
        {
            AddErrorMessage(errors, "startTime", "startTime must be earlier than endTime");
        }

        if (StartTime < minTime || StartTime > maxTime)
        {
            AddErrorMessage(errors, "startTime", $"startTime must be within time range of the forecasted data, '{minTime}' through '{maxTime}'");
        }

        if (EndTime < minTime || EndTime > maxTime)
        {
            AddErrorMessage(errors, "endTime", $"endTime must be within time range of the forecasted data, '{minTime}' through '{maxTime}'");
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
