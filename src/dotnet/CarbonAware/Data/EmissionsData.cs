using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonAware.Data
{
    [Serializable]
    public class EmissionsData
    {
        public string Location { get;  set; }
        public DateTime Time { get;  set; }
        public double Rating { get;  set; }

    }
}
