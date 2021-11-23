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
    public partial class CarbonAwareStaticJsonDataService : ICarbonAwareStaticDataService
    {

        private string _fileName { get; set; }

        public CarbonAwareStaticJsonDataService()
        {
            
        }

        public List<EmissionsData> GetData()
        {
            using StreamReader file = File.OpenText(_fileName);
            var jsonObject = JsonConvert.DeserializeObject<EmissionsJsonFile>(file.ReadToEnd());
            return jsonObject.Emissions;
        }

        public void LoadData(string location)
        {
            _fileName = location;
        }
    }
}
