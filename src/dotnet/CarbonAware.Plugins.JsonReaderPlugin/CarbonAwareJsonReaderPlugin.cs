using System.Collections;
using CarbonAware.Model;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Newtonsoft.Json;

namespace CarbonAware.Plugins.JsonReaderPlugin;

public class CarbonAwareJsonReaderPlugin : ICarbonAware
{
    private readonly ILogger<CarbonAwareJsonReaderPlugin> _logger;


    public CarbonAwareJsonReaderPlugin(ILogger<CarbonAwareJsonReaderPlugin> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<EmissionsData>> GetEmissionsDataAsync(IDictionary props)
    {
        var data = GetSampleJson();
        var l = props[CarbonAwareConstants.LOCATIONS] as IEnumerable<string>;
        var location = l.FirstOrDefault();
        return await Task.Run(() => data.Where(x => x.Location == location));
    }

    private string ReadFromResource(string key)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using Stream streamMetaData = assembly.GetManifestResourceStream(key);
        using StreamReader readerMetaData = new StreamReader(streamMetaData);
        return readerMetaData.ReadToEnd();
    }
 
    private List<EmissionsData> GetSampleJson()
    {
        var data = ReadFromResource("CarbonAware.Plugins.JsonReaderPlugin.test-data-azure-emissions.json");
        var jsonObject = JsonConvert.DeserializeObject<EmissionsJsonFile>(data);
        return jsonObject.Emissions;
    }
}
