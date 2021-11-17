using System.Collections.Generic;

namespace CarbonAware.Plugins.BasicJsonPlugin
{
    public class EmissionsJsonFile
    {
        public string Date { get; set; }
        public List<EmissionsData> Emissions { get; set; }
    }
}
