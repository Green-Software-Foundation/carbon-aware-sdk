namespace GSF.CarbonAware.Models;

public record EmissionsData
{
    public string? Location { get; init; }
    public DateTimeOffset Time { get; init; }
    public double Rating { get; init; }
    public TimeSpan Duration { get; set; }

    public static implicit operator EmissionsData(global::CarbonAware.Model.EmissionsData emissionsData) {
        return new EmissionsData
        {
            Duration = emissionsData.Duration,
            Location = emissionsData.Location,
            Rating = emissionsData.Rating,
            Time = emissionsData.Time
        };
    }
}
