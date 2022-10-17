namespace GSF.CarbonIntensity.Models;

public record EmissionsData
{
    public string? Location { get; init; }
    public DateTimeOffset Time { get; init; }
    public double Rating { get; init; }
    public TimeSpan Duration { get; set; }
}
