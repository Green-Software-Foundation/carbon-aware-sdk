namespace CarbonAware;

/// <summary>
/// Helper for intervals within Carbon Aware.
/// </summary>
public static class IntervalHelper
{

    /// <summary>
    /// Ensures the data is available between the range of time considering the duration of the data
    /// </summary>
    /// <param name="expandedData">Expanded data that includes the minimum sampling window</param>
    /// <param name="startTime">Original start time provided by user</param>
    /// <param name="endTime">Original end time provided by user</param>
    /// <returns>Filtered emissions data.</returns>
    public static IEnumerable<EmissionsData> FilterByDuration(IEnumerable<EmissionsData> expandedData, DateTimeOffset startTime, DateTimeOffset endTime, TimeSpan duration = default)
    {
        if (duration != default)
        {   // constant duration
            return expandedData.Where(d => (d.Time + duration) >= startTime && d.Time <= endTime);
        }
        return expandedData.Where(d => (d.Time + d.Duration) >= startTime && d.Time <= endTime);
    }

    /// <summary>
    /// Extends start and end times by subtracting/adding window respectively
    /// </summary>
    /// <param name="origStartTime">Original start time</param>
    /// <param name="origEndTime">Original end time</param>
    /// <param name="minWindow">Minute window to extend times by</param>
    /// <returns>Shifted times</returns>
    public static (DateTimeOffset, DateTimeOffset) ExtendTimeByWindow(DateTimeOffset origStartTime, DateTimeOffset origEndTime, double minWindow)
    {
        return (origStartTime.AddMinutes(-minWindow), origEndTime.AddMinutes(minWindow));
    }
}
