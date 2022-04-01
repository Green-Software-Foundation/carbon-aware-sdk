using System.Collections;
using CarbonAware.Model;
using Microsoft.Extensions.Logging;
using CarbonAware.Plugins.BasicJsonPlugin;

namespace CarbonAware.Plugins.NewBasicJsonPlugin;

public class CarbonAwareJsonReaderPlugin : ICarbonAware
{
    private readonly ILogger<CarbonAwareJsonReaderPlugin> Logger;
    private CarbonAwareStaticJsonDataService CarbonAwareDataService { get; }


    public CarbonAwareJsonReaderPlugin(ILogger<CarbonAwareJsonReaderPlugin> logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        CarbonAwareDataService = new CarbonAwareStaticJsonDataService();
        var file = "/workspaces/carbon-aware-sdk/src/dotnet/data/data-files/sample-emissions-data.json";
        CarbonAwareDataService.SetFileName(file);
    }

    public async Task<IEnumerable<EmissionsData>> GetEmissionsDataAsync(IDictionary props)
    {
        Logger.LogInformation("New Data");
        foreach (var k in props.Keys)
        {
            Logger.LogInformation($"Key: {k} - Value: {props[k]}");
        }
        return await Task.Run(() => this.CarbonAwareDataService.GetData());
    }

    public async Task<IEnumerable<EmissionsData>> GetEmissionsDataAsync(IDictionary props, Func<QueryObject, bool> filter)
    {
        return await Task.Run(() => this.CarbonAwareDataService.GetData().Where((x) => filter(new QueryObject { 
            Location = x.Location,
            Time = x.Time,
            Rating = x.Rating
        })));
    }


}