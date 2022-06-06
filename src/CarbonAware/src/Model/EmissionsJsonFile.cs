namespace CarbonAware.Model;

public class EmissionsJsonFile
{
    public string Date { get; set; } = "Undefined";
    public List<EmissionsData> Emissions { get; set; } = new List<EmissionsData>();
}
