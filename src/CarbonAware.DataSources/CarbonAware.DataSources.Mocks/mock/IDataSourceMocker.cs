namespace CarbonAware.DataSources.Mocks;

/// <summary>
/// This interface is used by the Integration Tests to set up data for different data sources
/// Each method corresponds to the dataset needed by a different integration test
/// </summary>
public interface IDataSourceMocker
{
    /// <summary>
    /// This sets up a data endpoint with certain parameters so that it can be pinged.
    /// </summary>
    /// <param name="start">The Start of the time interval</param>
    /// <param name="end">The end of the time interval</param>
    /// <param name="location">Which location the server is looking at.</param>
    public abstract void SetupDataMock(DateTimeOffset start, DateTimeOffset end, string location);

    public abstract void SetupForecastMock();
    public abstract void SetupBatchForecastMock();

    /// <summary>
    /// Initializes the DataSourceMocker with clean setup
    /// </summary>
    public abstract void Reset();

    /// <summary>
    /// Resets the DataSourceMocker and removes all stubs
    /// </summary>
    public abstract void Initialize();

    /// <summary>
    /// Disposal method
    /// </summary>
    void Dispose();
}