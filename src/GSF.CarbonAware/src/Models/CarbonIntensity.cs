namespace GSF.CarbonAware.Models;

public record CarbonIntensity
{
    /// <summary>the location name where workflow is run </summary>
    /// <example>eastus</example>
    public string Location { get; set; } = string.Empty;

    /// <summary>the requested carbon intensity per time range </summary>
    public IEnumerable<CarbonIntensityData> CarbonIntensityDataPoints { get; set; } = Enumerable.Empty<CarbonIntensityData>();
}

public record CarbonIntensityData
{
    /// <summary>the time at which the workflow we are measuring carbon intensity for started </summary>
    /// <example>2022-03-01T15:30:00Z</example>
    public DateTimeOffset StartTime { get; set; }

    /// <summary> the time at which the workflow we are measuring carbon intensity for ended</summary>
    /// <example>2022-03-01T18:30:00Z</example>
    public DateTimeOffset EndTime { get; set; }

    /// <summary>Value of the marginal carbon intensity in grams per kilowatt-hour.</summary>
    /// <example>345.434</example>
    public double Value { get; set; }

    public CarbonIntensityData(DateTimeOffset start, DateTimeOffset end, double value)
    {
        StartTime = start; EndTime = end; Value = value;
    }
}

