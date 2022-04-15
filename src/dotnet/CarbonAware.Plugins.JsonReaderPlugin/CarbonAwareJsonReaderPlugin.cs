using System.Collections;
using CarbonAware.Model;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Newtonsoft.Json;
using CarbonAware.Plugin;

namespace CarbonAware.Plugins.JsonReaderPlugin;

public class CarbonAwareJsonReaderPlugin : ICarbonAware
{
    public string Name => "CarbonAwareJsonReaderPlugin";

    public string Description => "Example plugin to read data from a json for Carbon Aware SDK";

    public string Author => "Microsoft";

    public string Version => "0.0.1";

    private readonly ILogger<CarbonAwareJsonReaderPlugin> _logger;

    private List<EmissionsData>? emissionsData;


    public CarbonAwareJsonReaderPlugin(ILogger<CarbonAwareJsonReaderPlugin> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmissionsData>> GetEmissionsDataAsync(IDictionary props)
    {
        List<EmissionsData>? emissionsData = GetSampleJson();
        if (emissionsData == null) {
            _logger.LogDebug("Emission data list is empty");
            return new List<EmissionsData>();
        }
        _logger.LogDebug("Total emission records retrieved " + emissionsData.Count);
        
        return await Task.FromResult(GetFilteredData(emissionsData, props));
    }

    private IEnumerable<EmissionsData> GetFilteredData(IEnumerable<EmissionsData> data, IDictionary props) {
        var location = props[CarbonAwareConstants.Locations] as IEnumerable<string>;
        List<String> locations = location !=null ? location.ToList() : new List<string>();

        var startDate = getStartDateFromProps(props);
        var endDate = props[CarbonAwareConstants.End];
        
        data = filterByLocation(data, locations);

        if (endDate != null)
        {
            data = filterByDateRange(data, startDate, endDate);
        }
        else
        {
            data  = data.Where(ed => ed.Time <= startDate);
        }

        if (data.Count() != 0)
        {
            data.MaxBy(ed => ed.Time);
        }

        return data;
    }

    private IEnumerable<EmissionsData> filterByDateRange(IEnumerable<EmissionsData> data, DateTime startDate, object endDate)
    {
        DateTime end;
        DateTime.TryParse(endDate.ToString(), out end);
        data = data.Where(ed => ed.TimeBetween(startDate, end));  

        return data;
    }

    private IEnumerable<EmissionsData> filterByLocation(IEnumerable<EmissionsData> data, List<string> locations)
    {
        if (locations.Any()) 
        {
            data = data.Where(ed => locations.Contains(ed.Location));
        }

        return data;
    }

    private DateTime getStartDateFromProps(IDictionary props) {
        var start = props[CarbonAwareConstants.Start];
        var startDate = DateTime.Now;
        if (start != null && !DateTime.TryParse(start.ToString(), out startDate))
        {
            startDate = DateTime.Now;
        }
       
        return startDate;
    }
    private string ReadFromResource(string key)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using Stream streamMetaData = assembly.GetManifestResourceStream(key) ?? throw new NullReferenceException("StreamMedataData is null");
        using StreamReader readerMetaData = new StreamReader(streamMetaData);
        return readerMetaData.ReadToEnd();
    }
 
    protected virtual List<EmissionsData>? GetSampleJson()
    {
        if(emissionsData == null || !emissionsData.Any()) {
            var data = ReadFromResource("CarbonAware.Plugins.JsonReaderPlugin.test-data-azure-emissions.json");
            var jsonObject = JsonConvert.DeserializeObject<EmissionsJsonFile>(data);
            emissionsData = jsonObject.Emissions;
        }
        return emissionsData;
    }
}
