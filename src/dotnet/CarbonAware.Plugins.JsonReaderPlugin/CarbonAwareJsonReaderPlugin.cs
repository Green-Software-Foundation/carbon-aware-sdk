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

        return await Task.Run(() => GetFilteredData(data, props));
    }

    private IEnumerable<EmissionsData> GetFilteredData(IEnumerable<EmissionsData> data, IDictionary props) {
        var l = props[CarbonAwareConstants.LOCATIONS] as IEnumerable<string>;
        List<String> locations = l !=null ? l.ToList() : new List<string>();

        var s = props[CarbonAwareConstants.START];
        DateTime start = s != null ? (DateTime)s : DateTime.Now;
        
        var e = props[CarbonAwareConstants.END];
        DateTime? end =  e != null ? (DateTime)e : null;

        var d = props[CarbonAwareConstants.DURATION];
        int durationMinutes =  d!= null ? (int)d : 0;
        
        foreach (var location in locations)
        {
            data = data.Where(ed => ed.Location.Equals(location));        

        }

        if (end != null)
        {
            data = data.Where(ed => ed.TimeBetween(start,end)).ToList();
        } else {
            data = data.Where(ed => ed.Time <= start);
        }

        if (data.Count() != 0)
        {
            data.MaxBy(ed => ed.Time);
        }

        return data;
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
