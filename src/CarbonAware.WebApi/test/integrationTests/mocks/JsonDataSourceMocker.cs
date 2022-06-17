using Microsoft.AspNetCore.Mvc.Testing;

namespace CarbonAware.WebApi.IntegrationTests;
public class JsonDataSourceMocker : IDataSourceMocker
{
    internal JsonDataSourceMocker() { }

    public void SetupDataMock(DateTimeOffset start, DateTimeOffset end, string location) { }

    public WebApplicationFactory<Program> OverrideWebAppFactory(WebApplicationFactory<Program> factory)
    {
        return factory;
    }

    public void Initialize() { }
    public void Reset() { }
    public void Dispose() { }
}