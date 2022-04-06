using NUnit.Framework;
using CarbonAware.Plugins.JsonReaderPlugin;
using CarbonAware;
using Moq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using CarbonAware.Model;
using System;
using System.Linq;

namespace CarbonAware.Plugins.JsonReaderPlugin.Tests;

public class JsonReaderPluginTests
{

    private ICarbonAware? jsonReaderPlugin;
    private Mock<ILogger<CarbonAwareJsonReaderPlugin>> MockLogger = new Mock<ILogger<CarbonAwareJsonReaderPlugin>>(); 


    [SetUp]
    public void Setup()
    {
        jsonReaderPlugin = new CarbonAwareJsonReaderPlugin(this.MockLogger.Object);
    }

    [Test]
    public void TestDataByLocation()
    {
        Dictionary<string, object> props = new Dictionary<string, object>();
        props[CarbonAwareConstants.LOCATIONS] = new List<string> { "eastus"};

        Task<IEnumerable<EmissionsData>> data = jsonReaderPlugin.GetEmissionsDataAsync(props);
        List<EmissionsData> emissionsData = data.Result.ToList();
        IEnumerable<EmissionsData> filteredData = data.Result.Where(ed => ed.Location.Equals("westus"));        

       Assert.IsEmpty(filteredData);

    }

    
    [Test]
    public void TestDataByLocationAndTimePeriod()
    {
        Dictionary<string, object> props = new Dictionary<string, object>();
        props[CarbonAwareConstants.LOCATIONS] = new List<string> { "eastus"};
        props[CarbonAwareConstants.START] = "2021-09-09";
        props[CarbonAwareConstants.END] = "2021-12-09";

        Task<IEnumerable<EmissionsData>> data = jsonReaderPlugin.GetEmissionsDataAsync(props);
        List<EmissionsData> emissionsData = data.Result.ToList();

        IEnumerable<EmissionsData> filteredData = data.Result.Where(ed => ed.Time > DateTime.Parse(props[CarbonAwareConstants.END].ToString()));        
        Assert.IsEmpty(filteredData);

        filteredData = data.Result.Where(ed => ed.Time < DateTime.Parse(props[CarbonAwareConstants.END].ToString()));        
        Assert.IsNotEmpty(filteredData);

    }

    
    // [Test]
    // public void TestDataByDuration()
    // {
    //     Dictionary<string, string> props = new Dictionary<string, string>();
    //     props[CarbonAwareConstants.LOCATIONS] = "eastus";
    //     props[CarbonAwareConstants.DURATION] = "60";

    //     Task<IEnumerable<EmissionsData>> data = jsonReaderPlugin.GetEmissionsDataAsync(props);
    //     List<EmissionsData> emissionsData = data.Result.ToList();
    //     IEnumerable<EmissionsData> filteredData1 = data.Result.Where(ed => ed.Location.Equals("westus"));        

    //     List<EmissionsData> filteredData = emissionsData.FindAll(x => (x.Location == "mmm"));
    //   //  Assert.IsEmpty(filteredData1);

    // }
}