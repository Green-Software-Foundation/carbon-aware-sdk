using CarbonAware.WebApi.Configuration;
using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using NUnit.Framework.Internal;

namespace CarbonAware.WepApi.UnitTests;

[TestFixture]
public class ServiceCollectionExtensionsTests
{
    [Test]
    public void AddMonitoringAndTelemetry_AddsServices_WithConnectionString()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();

        var inMemorySettings = new Dictionary<string, string>
        {
            { "CarbonAwareVars:TelemetryProvider", "ApplicationInsights" },
            { "ApplicationInsights_Connection_String", "AppInsightsConnectionString" }
        };
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        
        // Act & Assert
        Assert.DoesNotThrow(() => ServiceCollectionExtensions.AddMonitoringAndTelemetry(services, configuration));
        Assert.AreEqual(services.Count(), 43);
    }

    [Test]
    public void AddMonitoringAndTelemetry_AddsServices_WithInstrumentationKey()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();

        var inMemorySettings = new Dictionary<string, string>
        {
            { "CarbonAwareVars:TelemetryProvider", "ApplicationInsights" },
            { "AppInsights_InstrumentationKey", "AppInsightsInstrumentationKey" }
        };
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        
        // Act & Assert
        Assert.DoesNotThrow(() => ServiceCollectionExtensions.AddMonitoringAndTelemetry(services, configuration));
        Assert.AreEqual(services.Count(), 43);
    }

    [Test]
    public void AddMonitoringAndTelemetry_DoesNotAddServices_WithoutConfiguration()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();

        var inMemorySettings = new Dictionary<string, string>
        {
            { "CarbonAwareVars:TelemetryProvider", "ApplicationInsights" }
        };
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        
        // Act & Assert
        Assert.DoesNotThrow(() => ServiceCollectionExtensions.AddMonitoringAndTelemetry(services, configuration));
        Assert.AreEqual(services.Count(), 0);
    }

    [Test]
    public void AddMonitoringAndTelemetry_DoesNotAddServices_WithoutTelemetryProvider()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();

        var inMemorySettings = new Dictionary<string, string>{};
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        
        // Act & Assert
        Assert.DoesNotThrow(() => ServiceCollectionExtensions.AddMonitoringAndTelemetry(services, configuration));
        Assert.AreEqual(services.Count(), 0);
    }    

    [Test]
    public void CreateConsoleLogger_ReturnsILogger()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string>
        {
            { "Logging:LogLevel:Default", "Information" },
            { "Logging:LogLevel:Microsoft.AspNetCore", "Warning" }
        };
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act
        Microsoft.Extensions.Logging.ILogger logger = ServiceCollectionExtensions.CreateConsoleLogger(configuration);

        // Assert
        Assert.NotNull(logger);
    }
}