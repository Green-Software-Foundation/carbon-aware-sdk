using CarbonAware.WebApi.Configuration;
using NUnit.Framework;

namespace CarbonAware.WepApi.UnitTests;

class CarbonExporterConfigurationTests
{
    [TestCase(12, TestName = "AssertValid: PeriodInHours greater than 0")]
    [TestCase(1, TestName = "AssertValid: PeriodInHours equals to 1")]
    public void AssertValid_PeriodInHoursGreaterThanZero_DoesNotThrowException(int periodInHours)
    {
        CarbonExporterConfiguration carbonExporterConfiguration = new CarbonExporterConfiguration()
        {
            PeriodInHours = periodInHours
        };

        Assert.DoesNotThrow(() => carbonExporterConfiguration.AssertValid());
    }

    [TestCase(0, TestName = "AssertValid: PeriodInHours equals to 0")]
    public void AssertValid_PeriodInHoursGreaterThanZero_ThrowException(int periodInHours)
    {
        CarbonExporterConfiguration carbonExporterConfiguration = new CarbonExporterConfiguration()
        {
            PeriodInHours = periodInHours
        };

        Assert.Throws<ArgumentException>(() => carbonExporterConfiguration.AssertValid());
    }
}