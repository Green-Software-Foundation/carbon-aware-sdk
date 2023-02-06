using CarbonAware.Aggregators.CarbonAware;
using Moq;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.CommandLine.IO;
using CarbonAware.Aggregators.Forecast;
using CarbonAware.Aggregators.Emissions;
using CarbonAware.LocationSources;
using CarbonAware.Interfaces;

namespace CarbonAware.CLI.UnitTests;

/// <summary>
/// TestsBase for all CLI unit tests.
/// </summary>
public abstract class TestBase
{
    protected Mock<IForecastAggregator> _mockForecastAggregator = new();
    protected Mock<IEmissionsAggregator> _mockEmissionsAggregator = new();
    protected Mock<ILocationSource> _mockLocationSource = new();


    protected readonly TestConsole _console = new();

    protected InvocationContext SetupInvocationContext(Command command, string stringCommand)
    {
        _mockEmissionsAggregator = new Mock<IEmissionsAggregator>();
        _mockForecastAggregator = new Mock<IForecastAggregator>();
        _mockLocationSource = new Mock<ILocationSource>();

        var parser = new Parser(command);
        var parseResult = parser.Parse(stringCommand);
        var invocationContext = new InvocationContext(parseResult, _console);
        var mockServiceProvider = new Mock<IServiceProvider>();

        mockServiceProvider.Setup(x => x.GetService(typeof(IEmissionsAggregator)))
            .Returns(_mockEmissionsAggregator.Object);

        mockServiceProvider.Setup(x => x.GetService(typeof(IForecastAggregator)))
            .Returns(_mockForecastAggregator.Object);

        mockServiceProvider.Setup(x => x.GetService(typeof(ILocationSource)))
            .Returns(_mockLocationSource.Object);

        invocationContext.BindingContext.AddService<IServiceProvider>(_ => mockServiceProvider.Object);

        return invocationContext;
    }
}