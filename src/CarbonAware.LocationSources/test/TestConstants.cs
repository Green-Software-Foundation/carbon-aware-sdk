using CarbonAware.LocationSources.Model;
using CarbonAware.Model;

namespace CarbonAware.LocationSources.Test;

public static class Constants
{

    public static readonly NamedGeoposition EastUsRegion = new () {
                    Name = "test-eastus",
                    Latitude = "37.3719",
                    Longitude = "-71.8164"
                    };
    public static readonly NamedGeoposition WestUsRegion = new () {
                    Name = "test-westus",
                    Latitude = "37.783",
                    Longitude = "-121.417"
                };
    public static readonly NamedGeoposition NorthCentralRegion = new () {
                    Name = "test-northcentralus",
                    Latitude = "37.783",
                    Longitude = "-120.417"
                };
    public static readonly NamedGeoposition FakeRegion = new ();


    public static readonly Location LocationEastUs = new () {
                    Name = "test-eastus",
                    Latitude = 37.3719m,
                    Longitude = -71.8164m
                    };
    public static readonly Location LocationWestUs = new () {
                    Name = "test-westus",
                    Latitude = 37.783m,
                    Longitude = -121.417m
                };
    public static readonly Location FakeLocation = new () {
                    Name = "",
                    Latitude = 0.0m,
                    Longitude = 0.0m
                    };
}
