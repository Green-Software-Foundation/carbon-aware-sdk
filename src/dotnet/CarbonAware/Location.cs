using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonAware
{
    /// <summary>
    /// Basic geocoordinate representation 
    /// </summary>
    public class Location
    {
        public double Latitude { get; }
        public double Longitude { get; }

        public bool Equals(Location other)
        {
            return this.Latitude == other.Latitude && this.Longitude == other.Longitude;
        }

        public Location(string latitude, string longitude)
        {
            this.Latitude = double.Parse(latitude);
            this.Longitude = double.Parse(longitude);
        }

        public Location(double latitude, double longitude)
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

            throw new Exception($"Latitude,Longitude string is no in comma separated format: '{latitudeLongitude}'");
        }
    }
}
