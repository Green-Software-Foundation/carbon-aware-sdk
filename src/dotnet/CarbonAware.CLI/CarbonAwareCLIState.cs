namespace CarbonAwareCLI;

public class CarbonAwareCLIState
{
    public const string DEFAULT_CONFIG_FILE_NAME = "carbon-aware.json";

    public TimeOptionStates TimeOption { get; set; }
    public List<string> Locations { get; set; } = new List<string>();
    public DateTime Time { get; set; }
    public bool Lowest { get; set; }
    public LocationOptionStates LocationOption { get; set; }
    public bool Parsed { get; set; } = false;
    public string DataFile { get; set; }
    public bool Verbose { get; set; }
    public DateTime ToTime { get; internal set; }
    public string ConfigPath { get; set; } = DEFAULT_CONFIG_FILE_NAME;
}
