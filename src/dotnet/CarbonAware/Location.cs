using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonAware
{
    /// <summary>
    /// The location class by Vaughan
    /// </summary>
    public class Location
    {
        public string Latitude { get; }
        public string Longitude { get; }
        /// <summary>
        /// Vaughan's awesome summary
        /// </summary>
        public Location(string latitude, string longitude)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
        }

        public override string ToString()
        {
            return $"{this.Latitude},{this.Longitude}";
        }

        public static Location Parse(string latitudeLongitude)
        {
            var latLong = latitudeLongitude.Split(',');

            if(latLong.Length == 2)
            {
                return new Location(latLong[0], latLong[1]);
            }

            throw new Exception($"Latitude,Longitude string is no in comma seperated format: '{latitudeLongitude}'");
        }
    }
}
