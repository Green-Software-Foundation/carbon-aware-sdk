using CarbonAware.WebApi.Configuration;
using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using NUnit.Framework.Internal;
using OpenTelemetry.Trace;
using Microsoft.ApplicationInsights;

namespace CarbonAware.WepApi.UnitTests;

[TestFixture]
public class ServiceCollectionExtensionsTests
{
    [Test]
    public void AddMonitoringAndTelemetry_AddsServices_WithConnectionString()
    {
        // Arrange
        var services = new ServiceCollection();

        var inMemorySettings = new Dictionary<string, string>
        {
            { "CarbonAwareVars:TelemetryProvider", "ApplicationInsights" },
            { "ApplicationInsights_Connection_String", "AppInsightsConnectionString" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act & Assert
        Assert.DoesNotThrow(() => services.AddMonitoringAndTelemetry(configuration));
    }

    [Test]
    public void AddMonitoringAndTelemetry_AddsServices_WithInstrumentationKey()
    {
        // Arrange
        var services = new ServiceCollection();

        var inMemorySettings = new Dictionary<string, string>
        {
            { "CarbonAwareVars:TelemetryProvider", "ApplicationInsights" },
            { "AppInsights_InstrumentationKey", "AppInsightsInstrumentationKey" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act & Assert
        Assert.DoesNotThrow(() => services.AddMonitoringAndTelemetry(configuration));
    }

    [Test]
    public void AddMonitoringAndTelemetry_DoesNotAddServices_WithoutConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();

        var inMemorySettings = new Dictionary<string, string>
        {
            { "CarbonAwareVars:TelemetryProvider", "ApplicationInsights" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act & Assert
        Assert.DoesNotThrow(() => services.AddMonitoringAndTelemetry(configuration));
        Assert.That(services.Count, Is.EqualTo(0));
    }

    [Test]
    public void AddMonitoringAndTelemetry_DoesNotAddServices_WithoutTelemetryProvider()
    {
        // Arrange
        var services = new ServiceCollection();

        var inMemorySettings = new Dictionary<string, string>{};
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act & Assert
        Assert.DoesNotThrow(() => services.AddMonitoringAndTelemetry(configuration));
        Assert.Null(services.BuildServiceProvider().GetService<TelemetryClient>());
    }

    [Test]
    public void AddCarbonExporter_AddsServices_IsEnabledInConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();

        var inMemorySettings = new Dictionary<string, string>
        {
            { "CarbonAwareVars:EnableCarbonExporter", "true" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act & Assert
        Assert.DoesNotThrow(() => services.AddCarbonExporter(configuration));
        Assert.That(services.Count, Is.GreaterThan(0));
    }

    [Test]
    public void AddCarbonExporter_DoesNotAddServices_IsDisabledInConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();

        var inMemorySettings = new Dictionary<string, string>
        {
            { "CarbonAwareVars:EnableCarbonExporter", "false" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act & Assert
        Assert.DoesNotThrow(() => services.AddCarbonExporter(configuration));
        Assert.That(services.Count, Is.EqualTo(0));
    }


    [Test]
    public void AddCarbonExporter_DoesNotAddServices_WithoutConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();

        var inMemorySettings = new Dictionary<string, string>{};
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act & Assert
        Assert.DoesNotThrow(() => services.AddCarbonExporter(configuration));
        Assert.That(services.Count, Is.EqualTo(0));
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
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act
        Microsoft.Extensions.Logging.ILogger logger = ServiceCollectionExtensions.CreateConsoleLogger(configuration);

        // Assert
        Assert.That(logger, Is.Not.Null);
    }

    [Test]
    public void EnableTelemetryLogging_AddsServices_WithoutConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();

        var inMemorySettings = new Dictionary<string, string> { };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act & Assert
        Assert.DoesNotThrow(() => services.AddMonitoringAndTelemetry(configuration));
        Assert.NotNull(services.BuildServiceProvider().GetService<TracerProvider>());
    }

    [Test]
    public void EnableTelemetryLogging_AddsServices_IsEnabledInConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();

        var inMemorySettings = new Dictionary<string, string>
        {
            { "CarbonAwareVars:EnableTelemetryLogging", "true" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act & Assert
        Assert.DoesNotThrow(() => services.AddMonitoringAndTelemetry(configuration));
        Assert.NotNull(services.BuildServiceProvider().GetService<TracerProvider>());
    }

    [Test]
    public void EnableTelemetryLogging_AddsServices_IsDisabledInConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();

        var inMemorySettings = new Dictionary<string, string>
        {
            { "CarbonAwareVars:EnableTelemetryLogging", "false" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act & Assert
        Assert.DoesNotThrow(() => services.AddMonitoringAndTelemetry(configuration));
        Assert.Null(services.BuildServiceProvider().GetService<TracerProvider>());
    }
}