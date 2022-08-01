namespace CarbonAware.Model;

[Serializable]
public record EmissionsData
{
    ///<example> eastus </example>
    public string Location { get; set; }
    ///<example> 01-01-2022 </example>   
    public DateTimeOffset Time { get; set; }
    ///<example> 140.5 </example>
    public double Rating { get; set; }
    ///<example>1.12:24:02 </example>
    public TimeSpan Duration { get; set; }

    public bool TimeBetween(DateTimeOffset fromNotInclusive, DateTimeOffset? endInclusive)
    {
        if (endInclusive == null) return false;

        return Time > fromNotInclusive && Time <= endInclusive;
    }

}
