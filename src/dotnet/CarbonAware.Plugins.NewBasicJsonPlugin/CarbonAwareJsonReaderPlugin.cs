using System.Collections;
using CarbonAware.Model;
using CarbonAware.Plugins.BasicJsonPlugin;
using Microsoft.Extensions.Logging;

namespace CarbonAware.Plugins.NewBasicJsonPlugin;

public class CarbonAwareJsonReaderPlugin : ICarbonAware
{
    private readonly ILogger<CarbonAwareJsonReaderPlugin> Logger;
    private CarbonAwareStaticJsonDataService CarbonAwareDataService;


    public CarbonAwareJsonReaderPlugin(ILogger<CarbonAwareJsonReaderPlugin> logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        // CarbonAwareDataService = carbonAwareDataService;
        CarbonAwareDataService = new CarbonAwareStaticJsonDataService();
        var file = "/workspaces/carbon-aware-sdk/src/dotnet/data/data-files/sample-emissions-data.json";
        CarbonAwareDataService.SetFileName(file);
    }

    public async Task<IEnumerable<EmissionsData>> GetEmissionsDataAsync(Dictionary<string, object> props)
    {
        Logger.LogInformation("New Data");
        foreach (var k in props.Keys)
        {
            Logger.LogInformation($"Key: {k} - Value: {props[k]}");
        }
        
        List<string> locations = props.TryGetValue(CarbonAwareConstants.LOCATIONS, out object locs) ? (List<string>)locs : new List<string>();

        string location = locations.Count > 0 ? locations[0] : string.Empty;

        DateTime time = props.TryGetValue(CarbonAwareConstants.START, out object start) ? (DateTime)start : DateTime.Now;

        DateTime? toTime = props.TryGetValue(CarbonAwareConstants.END, out object end) ? (DateTime)end : null;

        int durationMinutes = props.TryGetValue(CarbonAwareConstants.DURATION, out object val) ? (int)val : 0;

        IEnumerable<EmissionsData> emissionsData = this.CarbonAwareDataService.GetData().Where(x => String.Equals(x.Location,location));
                                                                                        // .Where(x => x.TimeBetween(time, toTime));
                                                                                        

        return await Task.Run(() =>  emissionsData);
    }

}