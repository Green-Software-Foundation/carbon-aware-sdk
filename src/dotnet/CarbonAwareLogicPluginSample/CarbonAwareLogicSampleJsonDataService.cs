using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarbonAware;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CarbonAwareLogicPluginSample
{
    public class CarbonAwareLogicSampleJsonDataService : ICarbonDataService
    {
        private class JsonFile
        {
            public string Date { get; set; }
            public List<EmissionsData> Emissions { get; set; }
        }

        public List<EmissionsData> GetData()
        {
            using StreamReader file = File.OpenText(@"sample-emissions-data.json");
            var jsonObject = JsonConvert.DeserializeObject<JsonFile>(file.ReadToEnd());
            return jsonObject.Emissions;
        }
    }
}
