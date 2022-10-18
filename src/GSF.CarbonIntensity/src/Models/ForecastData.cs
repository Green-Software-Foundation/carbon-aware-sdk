using CarbonAware.Model;

namespace GSF.CarbonIntensity.Models;

// TODO document props
public record ForecastData
{
    public DateTimeOffset RequestedAt { get; init; }
    public DateTimeOffset GeneratedAt { get; init; }
    public IEnumerable<EmissionsData> EmissionsData { get; init; } = new List<EmissionsData>();
    public IEnumerable<EmissionsData> OptimalDataPoints { get; init; } = new List<EmissionsData>();
}