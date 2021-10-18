using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonAware
{
    public record EmissionsData
    {
        public Location Location;
        public DateTime Time;
        public double Rating;

        public static readonly EmissionsData None = new EmissionsData()
        {
            Location = null,
            Time = new DateTime(0),
            Rating = -1
        };
    }
}
