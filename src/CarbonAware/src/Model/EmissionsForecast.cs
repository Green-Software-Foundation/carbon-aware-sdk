using CarbonAware.Exceptions;

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
    public Location Location { get; set; } = new();

    /// <summary>
    /// Gets or sets the forecast data points.
    /// </summary>
    public IEnumerable<EmissionsData> ForecastData { get; set; } = new List<EmissionsData>();

    /// <summary>
    /// Gets or sets the optimal data points within the ForecastData set.
    /// </summary>
    public IEnumerable<EmissionsData> OptimalDataPoints { get; set; } = new List<EmissionsData>();


    public void Validate(DateTimeOffset dataStartAt, DateTimeOffset dataEndAt)
    {
        var errors = new Dictionary<string, List<string>>();
        var firstDataPoint = ForecastData.First();
        var lastDataPoint = ForecastData.Last();
        var minTime = firstDataPoint.Time;
        var maxTime = lastDataPoint.Time + lastDataPoint.Duration;

        if (dataStartAt >= dataEndAt)
        {
            AddErrorMessage(errors, "dataStartAt", "dataStartAt must be earlier than dataEndAt");
        }

        if (dataStartAt < minTime || dataStartAt > maxTime)
        {
            AddErrorMessage(errors, "dataStartAt", $"dataStartAt must be within time range of the forecasted data, '{minTime}' through '{maxTime}'");
        }

        if (dataEndAt < minTime || dataEndAt > maxTime)
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

    public TimeSpan GetDurationBetweenForecastDataPoints()
    {
        var firstPoint = ForecastData.FirstOrDefault();
        var secondPoint = ForecastData.Skip(1)?.FirstOrDefault();

        var first = firstPoint ?? throw new CarbonAwareException("First. Too few data points returned");
        var second = secondPoint ?? throw new CarbonAwareException("Second. Too few data points returned");

        // Handle chronological and reverse-chronological data by using `.Duration()` to get
        // the absolute value of the TimeSpan between the two points.
        return first.Time.Subtract(second.Time).Duration();
    }
}
