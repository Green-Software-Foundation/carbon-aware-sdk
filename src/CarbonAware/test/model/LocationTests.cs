using CarbonAware.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonAware.Tests.model;

class LocationTests
{
    [TestCase(34.123, "34.123", "en-US", TestName = "Lat/Long conversion to culture invariant string for en-US")]
    [TestCase(34.123, "34.123", "da-DK", TestName = "Lat/Long conversion to culture invariant string for da-DK")]
    public void LatLongConversionToInvariantCulture_WhenThreadCultureSpecified(decimal value, string expectedValue , string culture)
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
        Location _defaultLocation = new Location() { Name = "eastus", Latitude = value, Longitude = value};

        Assert.That(_defaultLocation.LatitudeAsCultureInvariantString(), Is.EqualTo(expectedValue));
        Assert.That(_defaultLocation.LongitudeAsCultureInvariantString(), Is.EqualTo(expectedValue));
    }
}
