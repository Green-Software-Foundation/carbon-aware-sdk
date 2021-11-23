using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarbonAware;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace CarbonAware.Plugins.BasicJsonPlugin
{
    public class CarbonAwareStaticJsonDataService : ICarbonAwareStaticDataService
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

        public void Configure(IConfigurationSection configuration)
        {
            _fileName = configuration.GetSection("data-file").Value;
            // TODO: File validation needs to be checked here now since moving it to configuration load
        }
    }
}
