using System.Collections.Generic;

namespace CarbonAware.Data.Sample
{
    public class EmissionsJsonFile
    {
        public string? Date { get; set; }
        public List<EmissionsData>? Emissions { get; set; }
    }
}
