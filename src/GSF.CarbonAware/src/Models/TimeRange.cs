namespace GSF.CarbonAware.Models;

public record TimeRange
{
    /// <summary>The time at which the workflow started </summary>
    /// <example>2022-03-01T15:30:00Z</example>
    public DateTimeOffset StartTime { get; set; }

    /// <summary> the time at which the workflow ended</summary>
    /// <example>2022-03-01T18:30:00Z</example>
    public DateTimeOffset EndTime { get; set; }

    public TimeRange(DateTimeOffset start, DateTimeOffset end)
    {
        StartTime = start; EndTime = end;
    }
}

