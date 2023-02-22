using GSF.CarbonAware.Models;

namespace CarbonAware.CLI.Model;
class EmissionsForecastDTO
{
    public DateTimeOffset GeneratedAt { get; set; }
    public DateTimeOffset RequestedAt { get; set; } = DateTimeOffset.UtcNow;
    public string Location { get; set; } = string.Empty;
    public DateTimeOffset DataStartAt { get; set; }
    public DateTimeOffset DataEndAt { get; set; }
    public int WindowSize { get; set; }
    public IEnumerable<EmissionsDataDTO>? OptimalDataPoints { get; set; }
    public IEnumerable<EmissionsDataDTO>? ForecastData { get; set; }

    public static EmissionsForecastDTO FromEmissionsForecast(EmissionsForecast emissionsForecast, DateTimeOffset? requestedAt, DateTimeOffset? startTime, DateTimeOffset? endTime)
    {
        return new EmissionsForecastDTO
        {
            Location = emissionsForecast.Location!,
            RequestedAt = requestedAt ?? DateTimeOffset.UtcNow,
            DataStartAt = startTime ?? emissionsForecast.EmissionsDataPoints.First().Time,
            DataEndAt = endTime ?? emissionsForecast.EmissionsDataPoints.Last().Time,
            GeneratedAt = emissionsForecast.GeneratedAt,
            ForecastData = emissionsForecast.EmissionsDataPoints.Select(d => (EmissionsDataDTO)d),
            OptimalDataPoints = emissionsForecast.OptimalDataPoints.Select(d => (EmissionsDataDTO)d)
        };
    }
}