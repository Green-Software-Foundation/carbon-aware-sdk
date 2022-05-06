namespace CarbonAware.Model;

[Serializable]
public record EmissionsData
{
    public string Location { get; set; }
    public DateTime Time { get; set; }
    public double Rating { get; set; }

    /// <summary>
    /// DataSource from this emission data.
    /// </summary>
    public string DataSource { get; set; }


    public bool TimeBetween(DateTime fromNotInclusive, DateTime? endInclusive)
    {
        if (endInclusive == null) return false;

        return Time > fromNotInclusive && Time <= endInclusive;
    }

}
