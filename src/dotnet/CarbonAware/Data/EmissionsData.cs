namespace CarbonAware.Data;

[Serializable]
public record EmissionsData
{
    public string Location { get; set; }
    public DateTime Time { get; set; }
    public double Rating { get; set; }

}
