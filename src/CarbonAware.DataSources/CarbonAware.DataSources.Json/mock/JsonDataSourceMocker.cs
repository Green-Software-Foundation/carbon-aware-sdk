using CarbonAware.DataSources.Mocks;

namespace CarbonAware.DataSources.Json.Mocks;
public class JsonDataSourceMocker : IDataSourceMocker
{
    public JsonDataSourceMocker() { }

    public void SetupDataMock(DateTimeOffset start, DateTimeOffset end, string location) { }
    public void SetupForecastMock() { }
    public void Initialize() { }
    public void Reset() { }
    public void Dispose() { }

    public void SetupBatchForecastMock()
    {
        throw new NotImplementedException();
    }
}