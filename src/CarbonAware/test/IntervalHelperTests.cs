using CarbonAware.Model;

namespace CarbonAware.Tests;

[TestFixture]
public class IntervalHelperTests
{
    private readonly DateTimeOffset startDateTimeOffset = new (2021, 9, 1, 9, 40, 0, TimeSpan.Zero);
    private readonly DateTimeOffset endDateTimeOffset = new (2021, 9, 1, 10, 20, 0, TimeSpan.Zero);

    /// <summary>
    /// Test exit cases of min sampling filtering
    /// </summary>
    [Test]
    public void TestFilterByDuration()
    {
        // Sample data from 8:30 to 10:30
        var data = new EmissionsData[5]
        {
            new EmissionsData {
                Location = "eastus",
                Time = new DateTimeOffset(2021,9,1,8,30,0, TimeSpan.Zero),
                Duration = TimeSpan.FromMinutes(30)
            },
            new EmissionsData {
                Location = "eastus",
                Time = new DateTimeOffset(2021,9,1,9,0,0, TimeSpan.Zero),
                Duration = TimeSpan.FromMinutes(30)
            },
            new EmissionsData {
                Location = "eastus",
                Time = new DateTimeOffset(2021,9,1,9,30,0, TimeSpan.Zero),
                Duration = TimeSpan.FromMinutes(30)
            },
            new EmissionsData {
                Location = "eastus",
                Time = new DateTimeOffset(2021,9,1,10,0,0, TimeSpan.Zero),
                Duration = TimeSpan.FromMinutes(30)
            },
            new EmissionsData {
                Location = "eastus",
                Time = new DateTimeOffset(2021,9,1,10,30,0, TimeSpan.Zero),
                Duration = TimeSpan.FromMinutes(30)
            }
        };

        // If pass in empty data, will just return empty data
        var emptyResult = IntervalHelper.FilterByDuration(Enumerable.Empty<EmissionsData>(), startDateTimeOffset, endDateTimeOffset);
        Assert.False(emptyResult.Any());

        // If pass in duration, will ignore data value. With 45 min duration, captures 3 data points
        var constantDuration = IntervalHelper.FilterByDuration(data, startDateTimeOffset, endDateTimeOffset, TimeSpan.FromMinutes(45));
        Assert.True(constantDuration.Count() == 3);

        // If don't pass in duration, will lookup value in data. WIth included 30 min duration, captures 2 data points
        var minWindowValid = IntervalHelper.FilterByDuration(data, startDateTimeOffset, endDateTimeOffset);
        Assert.True(minWindowValid.Count() == 2);
    }

    /// <summary>
    /// Test shift date functionality
    /// </summary>
    [Test]
    public void TestExtendTimeByWindo()
    {
        int minuteWindow = 30;
        (DateTimeOffset, DateTimeOffset) shifted = IntervalHelper.ExtendTimeByWindow(startDateTimeOffset, endDateTimeOffset, minuteWindow);
        Assert.True(shifted.Item1.AddMinutes(minuteWindow).Equals(startDateTimeOffset)); // start shifted back by minuteWindow
        Assert.True(shifted.Item2.AddMinutes(-minuteWindow).Equals(endDateTimeOffset)); // end shifted forward by minuteWindow
    }
}
