using CarbonAware.LocationSources.Exceptions;
using CarbonAware.LocationSources.Model;
using CarbonAware.Model;
using NUnit.Framework;

namespace CarbonAware.LocationSources.Test.Model;
class NamedGeopositionTests
{
    [TestCase("eastus", "36.66", "-170.34", TestName = "No error when all parameters are provided")]
    [TestCase("", "36.66", "-170.34", TestName = "No error when only latitude and longitude is provided")]
    [TestCase("eastus", "", "", TestName = "No error when only region name is provided")]
    [TestCase("eastus", "", "-170.34", TestName = "No error when region name and longitude are provided, but latitude is empty")]
    [TestCase("eastus", "36.66", "", TestName = "No error when region name and latitude are provided, but longitude is empty")]
    public void AssertValid_NoError_WhenValidInputProvided(string name, string latitude, string longitude)
    {
        NamedGeoposition namedGeoposition = new() { Name = name, Latitude = latitude, Longitude= longitude };
        namedGeoposition.AssertValid();
    }

    [TestCase("", "", "", TestName = "Throws LocationConversionException when no parameters are provided")]
    [TestCase("", "", "-170.34", TestName = "Throws LocationConversionException when region name and latitude is not provided")]
    [TestCase("", "36.66", "", TestName = "Throws LocationConversionException when region name and longitude is not provided")]
    public void AssertValid_ThrowsLocationConversionException_WhenInvalidInputProvided(string name, string latitude, string longitude)
    {
        NamedGeoposition namedGeoposition = new() { Name = name, Latitude = latitude, Longitude = longitude };
        Assert.Throws<LocationConversionException>(()=> namedGeoposition.AssertValid());
    }

    [TestCase("", "", "", TestName = "Converts NamedGeoposition to Location when no parameters are present")]
    [TestCase("eastus", "36.66", "-170.23", TestName = "Converts NamedGeoposition to Location when all parameters present")]
    [TestCase("", "", "-170.34", TestName = "Converts NamedGeoposition to Location when only longitude is present")]
    [TestCase("", "36.66", "", TestName = "Converts NamedGeoposition to Location when only latitude is present")]
    [TestCase("eastus", "", "-170.34", TestName = "Converts NamedGeoposition to Location when name and longitude are present")]
    [TestCase("eastus", "36.66", "", TestName = "Converts NamedGeoposition to Location when name and latitude are present")]
    public void Location_ConversionValid(string name, string latitude, string longitude)
    {
        NamedGeoposition namedGeoposition = new() { Name = name, Latitude = latitude, Longitude = longitude };
        Location location = (Location) namedGeoposition;

        if (!string.IsNullOrWhiteSpace(latitude))
        {
            Assert.NotNull(location.Latitude);
            Assert.AreEqual(latitude, location.Latitude.ToString());
        }
        else
        {
            Assert.Null(location.Latitude);
        }

        if (!string.IsNullOrWhiteSpace(longitude))
        {
            Assert.NotNull(location.Longitude);
            Assert.AreEqual(longitude, location.Longitude.ToString());
        }
        else
        {
            Assert.Null(location.Longitude);
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            Assert.NotNull(location.Name);
            Assert.AreEqual(name, location.Name);
        }
        else
        {
            Assert.IsEmpty(location.Name);
        }
    }

}
