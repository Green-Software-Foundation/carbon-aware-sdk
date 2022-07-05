using CarbonAware.Model;

namespace CarbonAware.LocationSources.Azure.Test;

public static class Constants
{

    public static readonly NamedGeoposition EastUsRegion = new () {
                    RegionName = "eastus",
                    Latitude = "37.3719",
                    Longitude = "-79.8164"
                    };
    public static readonly NamedGeoposition WestUsRegion = new () {
                    RegionName = "westus",
                    Latitude = "37.783",
                    Longitude = "-122.417"
                };
    public static readonly NamedGeoposition NorthCentralRegion = new () {
                    RegionName = "northcentralus",
                    Latitude = "37.783",
                    Longitude = "-122.417"
                };

    public static readonly Location LocationEastUs = new () {
                    RegionName = "eastus",
                    Latitude = 37.3719m,
                    Longitude = -79.8164m,
                    LocationType = LocationType.Geoposition
                    };
    public static readonly Location LocationWestUs = new () {
                    RegionName = "westus",
                    Latitude = 37.783m,
                    Longitude = -122.417m,
                    LocationType = LocationType.Geoposition
                };
    public static readonly Location LocationNorthCentral = new () {
                    RegionName = "northcentralus",
                    Latitude = 37.783m,
                    Longitude = -122.417m,
                    LocationType = LocationType.Geoposition
                };            
}
