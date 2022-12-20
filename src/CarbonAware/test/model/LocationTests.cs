using CarbonAware.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonAware.Tests.model;

public class LocationTests
{
    private static Location _defaultLocation = new Location() { Name = "eastus", Latitude = 34.123m, Longitude = 123.456m };

    [Test]
    public void LatLongConversionToInvariantCulture_WhenThreadCultureSpecified()
    {
        var expectedLatitude = "34.123";
        var expectedLongitude = "123.456";

        Thread.CurrentThread.CurrentCulture = new CultureInfo("da-DK");

        Assert.That(_defaultLocation.LatitudeAsCultureInvariantString(), Is.EqualTo(expectedLatitude));
        Assert.That(_defaultLocation.LongitudeAsCultureInvariantString(), Is.EqualTo(expectedLongitude));
    }

    [Test]
    public void LatLongConversionToString_WhenThreadCultureSpecified()
    {
        var expectedLatitude = "34,123";
        var expectedLongitude = "123,456";

        Thread.CurrentThread.CurrentCulture = new CultureInfo("da-DK");

        Assert.That(_defaultLocation.Latitude.ToString(), Is.EqualTo(expectedLatitude));
        Assert.That(_defaultLocation.Longitude.ToString(), Is.EqualTo(expectedLongitude));
    }
}
