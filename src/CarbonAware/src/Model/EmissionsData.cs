namespace CarbonAware.Model;

[Serializable]
public record EmissionsData
{
    public string Location { get; set; }
    public DateTimeOffset Time { get; set; }
    public double Rating { get; set; }
    public TimeSpan Duration { get; set; }

    public bool TimeBetween(DateTimeOffset fromNotInclusive, DateTimeOffset? endInclusive)
    {
        if (endInclusive == null) return false;

        return Time > fromNotInclusive && Time <= endInclusive;
    }

}
