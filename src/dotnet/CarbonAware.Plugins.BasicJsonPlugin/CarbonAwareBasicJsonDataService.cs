using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarbonAware;
using Newtonsoft.Json;

namespace CarbonAware.Plugins.BasicJsonPlugin
{
    public class CarbonAwareStaticJsonDataService : ICarbonAwareStaticDataService
    {
        private class JsonFile
        {
            public string Date { get; set; }
            public List<EmissionsData> Emissions { get; set; }
        }

        private string _fileName { get; }

        public CarbonAwareStaticJsonDataService(string fileName)
        {
            _fileName = fileName;
        }

        public List<EmissionsData> GetData()
        {
            using StreamReader file = File.OpenText(_fileName);
            var jsonObject = JsonConvert.DeserializeObject<JsonFile>(file.ReadToEnd());
            return jsonObject.Emissions;
        }
    }
}
