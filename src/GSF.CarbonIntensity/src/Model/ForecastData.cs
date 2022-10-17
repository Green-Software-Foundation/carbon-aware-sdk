namespace GSF.CarbonIntensity.Model;

// TODO document props
// TODO EmissiosData and OptimalDataPoints
public record ForecastData
{
    public DateTimeOffset RequestedAt { get; set; }
    public DateTimeOffset GeneratedAt { get; set; }
    public string? Location { get; set; }
    public DateTimeOffset DataStartAt { get; set; }
    public DateTimeOffset DataEndAt { get; set; }
    public TimeSpan WindowSize { get; set; }
    public IEnumerable<string> EmissionsData { get; set; } = Array.Empty<string>();
    public IEnumerable<string> OptimalDataPoints { get; set; } = Array.Empty<string>();
}
