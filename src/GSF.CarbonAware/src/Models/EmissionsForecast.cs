namespace GSF.CarbonAware.Models;

public record EmissionsForecast
{
    public string Location { get; set; } = String.Empty;
    public DateTimeOffset GeneratedAt { get; init; }
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
