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
    public class AzureRegionStaticDataService 
    {
        public class AzureRegionData
        {
            public string name { get; set; }
        }

        private string _fileName { get; }

        public AzureRegionStaticDataService(string fileName)
        {
            _fileName = fileName;
        }

        public List<AzureRegionData> GetRegionData()
        {
            using StreamReader file = File.OpenText(_fileName);
            var jsonObject = JsonConvert.DeserializeObject<List<AzureRegionData>>(file.ReadToEnd());
           
            return jsonObject;
        }

        public List<EmissionsData> GenerateDummyData(List<AzureRegionData> regionData)
        {
            List<EmissionsData> emData = new List<EmissionsData>();
            var ran = new Random(DateTime.Now.Millisecond);

            foreach (var region in regionData)
            {
                for (var i = 0; i < 24; i++)
                {
                    var e = new EmissionsData()
                    {
                        Time = DateTime.Now + TimeSpan.FromHours(i),
                        Location = region.name,
                        Rating = ran.Next(100)
                    };
                    emData.Add(e);
                }
            }

            return emData;
        }
    }
}
