namespace CarbonAware.Data; 

public class EmissionsJsonFile
{
    #nullable enable
    public string? Date { get; set; }
    public List<EmissionsData>? Emissions { get; set; }
}
