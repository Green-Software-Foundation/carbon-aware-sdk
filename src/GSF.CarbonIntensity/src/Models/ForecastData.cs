namespace GSF.CarbonIntensity.Models;

// TODO document props
// TODO EmissiosData and OptimalDataPoints
public record ForecastData
{
    public DateTimeOffset RequestedAt { get; init; }
    public DateTimeOffset GeneratedAt { get; init; }
    public string? Location { get; init; }
    public DateTimeOffset DataStartAt { get; init; }
    public DateTimeOffset DataEndAt { get; init; }
    public TimeSpan WindowSize { get; init; }
    public IEnumerable<string> EmissionsData { get; init; } = Array.Empty<string>();
    public IEnumerable<string> OptimalDataPoints { get; init; } = Array.Empty<string>();
}
