using CarbonAware.Model;
using Microsoft.Extensions.Configuration;

namespace CarbonAware.Tests;

public class EmissionsDataTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void TestTimeBetween()
    {
        var data = new EmissionsData()
        {
            Location = "Sydney",
            Rating = 100,
            Time = DateTime.Now
        };
        var beforeNow = DateTime.Now - TimeSpan.FromHours(1);
        var wellBeforeNow = beforeNow - TimeSpan.FromHours(1);
        var afterNow = DateTime.Now + TimeSpan.FromHours(1);

        // Time is greater than start and less than end > && <
        Assert.True(data.TimeBetween(beforeNow, afterNow));

        // Time is greater than start and equal to end > && ==
        Assert.True(data.TimeBetween(beforeNow, data.Time));

        // Time is after both times > && > 
        Assert.False(data.TimeBetween(wellBeforeNow, beforeNow));

        // Time is the start time == && <= 
        Assert.False(data.TimeBetween(data.Time, afterNow));

        // Should always be false with no end date
        Assert.False(data.TimeBetween(wellBeforeNow, null));

        // Should always be false if Time is less than both < && < 
        Assert.False(data.TimeBetween(afterNow, afterNow));
    }
}
