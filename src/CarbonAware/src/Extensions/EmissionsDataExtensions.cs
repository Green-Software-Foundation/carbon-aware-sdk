namespace CarbonAware.Extensions;

/// <summary>
/// Extension class for working with EmissionsData objects.
/// </summary>
public static class EmissionsDataExtensions
{
    /// <summary>
    /// Projects the data as a rolling average for a specified window size. 
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This type of rolling average windowing is necessary for determining the optimal period for any workload whose  
    ///         duration is longer than the granularity of the EmissionsData. Take the following examples of values for 5 
    ///         minute increments that produce false "optimal" answers for a 10 minute long workload:
    ///     </para>
    ///     <para>
    ///         [10, 20, 30, 40, 5] - The optimal single point is the last one, but our workload cannot start at that time and finish within the dataset.
    ///         [15, 25, 35, 22.5]  - This is a 10 minute rolling average of the above data, where the lowest value is both optimal and viable for our workload.
    ///     </para>
    ///     <para>
    ///         [5, 55, 10, 10, 10] - The optimal single point is the first one, but our workload started at that time will have an suboptimal average value (AVG(5,55) == 30).
    ///         [30, 32.5, 10, 10]  - This is a 10 minute rolling average of the above data, where the lowest value is both optimal and viable for our workload.
    ///     </para>
    ///     <para>
    ///         [12, 11, 10, 12, 13] - The optimal single point is the center one, but our workload started at that time will have an suboptimal average value (AVG(10,12) == 11).
    ///         [11.5, 10.5, 11, 12.5]  - This is a 10 minute rolling average of the above data, where the lowest value is both optimal and viable for our workload.
    ///     </para>
    /// </remarks>
    /// <param name="data">The IEnumerable<EmissionsData> being operated on.</param>
    /// <param name="windowSize">The duration of the window to be averaged.</param>
    /// <param name="tickSize">The duration the windows slides forward before calculating the next average.</param>
    /// <returns>An enumerable of emissions data objects, each representing a single average window.</returns>
    public static IEnumerable<EmissionsData> RollingAverage(this IEnumerable<EmissionsData> data, TimeSpan windowSize = default, TimeSpan tickSize = default)
    {
        if (data.Count() == 0){ yield break; }

        if (windowSize == default)
        {
           foreach(var d in data){ yield return d; }
           yield break;
        }

        var q = new Queue<EmissionsData>();
        var _data = data.GetEnumerator();
        _data.MoveNext();
        EmissionsData current = _data.Current;
        EmissionsData last = null;

        if (tickSize == default)
        {
            tickSize = (current.Duration > TimeSpan.Zero) ? current.Duration : throw new Exception("RollingAverage tickSize must be > 0");
        }

        // Set initial rolling average window
        DateTimeOffset windowStartTime = current.Time;
        DateTimeOffset windowEndTime = windowStartTime + windowSize;

        while (current != null)
        {
            // Enqueue data points relevant to current rolling average window
            while (current != null && windowEndTime > current.Time)
            {
                q.Enqueue(current);
                last = current;
                _data.MoveNext();
                current = _data.Current;
            }

            // Calculate average for everything in the queue if we enqueued enough data points to cover the window
            if (last != null && last.Time + last.Duration >= windowEndTime)
            {
                yield return AverageOverPeriod(q, windowStartTime, windowEndTime);
            }

            // Set bounds for the next window
            windowStartTime = windowStartTime + tickSize;
            windowEndTime = windowStartTime + windowSize;

            // Dequeue items not needed for next window average
            var peek = q.Peek();
            while (peek != null && peek.Time + peek.Duration < windowStartTime)
            {
                q.Dequeue();
                peek = q.Count == 0 ? null : q.Peek();
            }
        }
    }

    private static EmissionsData AverageOverPeriod(this IEnumerable<EmissionsData> data, DateTimeOffset startTime, DateTimeOffset endTime)
    {
        EmissionsData newDataPoint = new EmissionsData()
        {
            Time = startTime,
            Duration = (endTime - startTime),
            Rating = 0.0,
            Location = data.First().Location
        };
        foreach (var current in data)
        {
            if (current.Time + current.Duration > startTime && current.Time < endTime)
            {
                var lowerBound = (startTime >= current.Time) ? startTime : current.Time;
                var upperBound = (endTime < current.Time + current.Duration) ? endTime : current.Time + current.Duration;
                newDataPoint.Rating += current.Rating * (upperBound - lowerBound) / newDataPoint.Duration;
            }
        }

        return newDataPoint;
    }
}