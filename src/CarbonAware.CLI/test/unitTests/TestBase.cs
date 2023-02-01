using Moq;
using GSF.CarbonAware.Handlers;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.CommandLine.IO;

namespace CarbonAware.CLI.UnitTests;

/// <summary>
/// TestsBase for all CLI unit tests.
/// </summary>
internal abstract class TestBase
{
    protected Mock<IForecastHandler> _mockForecastHandler = new();
    protected Mock<IEmissionsHandler> _mockEmissionsHandler = new();
    protected Mock<ILocationHandler> _mockLocationHandler = new();


    protected readonly TestConsole _console = new();

    protected InvocationContext SetupInvocationContext(Command command, string stringCommand)
    {
        _mockEmissionsHandler = new Mock<IEmissionsHandler>();
        _mockForecastHandler = new Mock<IForecastHandler>();
        _mockLocationHandler = new Mock<ILocationHandler>();

        var parser = new Parser(command);
        var parseResult = parser.Parse(stringCommand);
        var invocationContext = new InvocationContext(parseResult, _console);
        var mockServiceProvider = new Mock<IServiceProvider>();

        mockServiceProvider.Setup(x => x.GetService(typeof(IEmissionsHandler)))
            .Returns(_mockEmissionsHandler.Object);

        mockServiceProvider.Setup(x => x.GetService(typeof(IForecastHandler)))
            .Returns(_mockForecastHandler.Object);

        mockServiceProvider.Setup(x => x.GetService(typeof(ILocationHandler)))
            .Returns(_mockLocationHandler.Object);

        invocationContext.BindingContext.AddService<IServiceProvider>(_ => mockServiceProvider.Object);

        return invocationContext;
    }
}