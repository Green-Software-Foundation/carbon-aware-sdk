namespace GSF.CarbonIntensity.Models;

public record EmissionsForecast
{
    public DateTimeOffset RequestedAt { get; init; }
    public DateTimeOffset GeneratedAt { get; init; }
    public IEnumerable<EmissionsData> EmissionsDataPoints { get; init; } = Array.Empty<EmissionsData>();
    public IEnumerable<EmissionsData> OptimalDataPoints { get; init; } = Array.Empty<EmissionsData>();
}
