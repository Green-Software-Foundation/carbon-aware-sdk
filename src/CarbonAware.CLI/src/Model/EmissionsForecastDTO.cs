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

    public static explicit operator EmissionsForecastDTO(global::GSF.CarbonAware.Models.EmissionsForecast emissionsForecast)
    {
        EmissionsForecastDTO forecast = new()
        {
            RequestedAt = emissionsForecast.RequestedAt,
            GeneratedAt = emissionsForecast.GeneratedAt,
            ForecastData = emissionsForecast.EmissionsDataPoints.Select(d => (EmissionsDataDTO)d!),
            OptimalDataPoints = emissionsForecast.OptimalDataPoints.Select(d => (EmissionsDataDTO)d)!
        };
        return forecast;
    }
 }