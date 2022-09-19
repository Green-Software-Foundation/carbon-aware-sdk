using CarbonAware.Aggregators.CarbonAware;
using Moq;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.CommandLine.IO;

namespace CarbonAware.CLI.UnitTests;

/// <summary>
/// TestsBase for all CLI unit tests.
/// </summary>
public abstract class TestBase
{
    protected Mock<ICarbonAwareAggregator> _mockCarbonAwareAggregator = new Mock<ICarbonAwareAggregator>();
    protected readonly TestConsole _console = new();

    protected InvocationContext SetupInvocationContext(Command command, string stringCommand)
    {
        _mockCarbonAwareAggregator = new Mock<ICarbonAwareAggregator>();

        var parser = new Parser(command);
        var parseResult = parser.Parse(stringCommand);
        var invocationContext = new InvocationContext(parseResult, _console);
        var mockServiceProvider = new Mock<IServiceProvider>();

        mockServiceProvider.Setup(x => x.GetService(typeof(ICarbonAwareAggregator)))
            .Returns(_mockCarbonAwareAggregator.Object);
        
        invocationContext.BindingContext.AddService<IServiceProvider>(_ => mockServiceProvider.Object);

        return invocationContext;
    }
}