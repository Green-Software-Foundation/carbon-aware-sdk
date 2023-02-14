namespace GSF.CarbonAware.Models;

public record EmissionsForecast
{
    public string? Location { get; init; }
    public DateTimeOffset RequestedAt { get; init; }
    public DateTimeOffset GeneratedAt { get; init; }
    public IEnumerable<EmissionsData> EmissionsDataPoints { get; init; } = Array.Empty<EmissionsData>();
    public IEnumerable<EmissionsData> OptimalDataPoints { get; init; } = Array.Empty<EmissionsData>();

    public static implicit operator EmissionsForecast(global::CarbonAware.Model.EmissionsForecast emissionsForecast) {
        return new EmissionsForecast {
            Location = emissionsForecast.Location.Name,
            GeneratedAt = emissionsForecast.GeneratedAt,
            EmissionsDataPoints = emissionsForecast.ForecastData.Select(x => (EmissionsData) x),
            OptimalDataPoints = emissionsForecast.OptimalDataPoints.Select(x => (EmissionsData) x)
        };
    }
}
