namespace GSF.CarbonAware.Models;

public record EmissionsForecast
{
    private readonly DateTimeOffset _requestedAt;
    private readonly DateTimeOffset _generatedAt;
    public DateTimeOffset RequestedAt { get => _requestedAt; init => _requestedAt = value.ToUniversalTime(); }
    public DateTimeOffset GeneratedAt { get => _generatedAt; init => _generatedAt = value.ToUniversalTime(); }
    public string Location { get; set; } = String.Empty;
    public IEnumerable<EmissionsData> EmissionsDataPoints { get; init; } = Array.Empty<EmissionsData>();
    public IEnumerable<EmissionsData> OptimalDataPoints { get; init; } = Array.Empty<EmissionsData>();

    public static implicit operator EmissionsForecast(global::CarbonAware.Model.EmissionsForecast emissionsForecast) {
        return new EmissionsForecast {
            GeneratedAt = emissionsForecast.GeneratedAt,
            EmissionsDataPoints = emissionsForecast.ForecastData.Select(x => (EmissionsData) x),
            OptimalDataPoints = emissionsForecast.OptimalDataPoints.Select(x => (EmissionsData) x),
            Location = emissionsForecast.Location.Name!,
        };
    }
}
