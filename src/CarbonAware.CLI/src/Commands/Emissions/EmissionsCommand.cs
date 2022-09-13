using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.CLI.Common;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json;

namespace CarbonAware.CLI.Commands.Emissions;

class EmissionsCommand : Command
{
    private readonly Option<string[]> _requiredLocation = CommonOptions.RequiredLocationOption;
    private readonly Option<DateTimeOffset?> _startTime = new Option<DateTimeOffset?>(
            new string[] { "--start-time", "-s" },
            LocalizableStrings.StartTimeDescription)
            {
                IsRequired = false,
                Arity = ArgumentArity.ExactlyOne,
            };
    private readonly Option<DateTimeOffset?> _endTime = new Option<DateTimeOffset?>(
             new string[] { "--end-time", "-e" },
            LocalizableStrings.EndTimeDescription)
            {
                IsRequired = false,
                Arity = ArgumentArity.ExactlyOne,
            };
    public EmissionsCommand() : base("emissions", LocalizableStrings.EmissionsCommandDescription)
    {
        AddOption(_requiredLocation);
        AddOption(_startTime);
        AddOption(_endTime);
        this.SetHandler(this.Run);
    }

    private async Task Run(InvocationContext context)
    {
        // Get aggregator via DI.
        var serviceProvider = context.BindingContext.GetService(typeof(IServiceProvider)) as IServiceProvider ?? throw new NullReferenceException("ServiceProvider not found");
        var aggregator = serviceProvider.GetService(typeof(ICarbonAwareAggregator)) as ICarbonAwareAggregator ?? throw new NullReferenceException("CarbonAwareAggregator not found");

        // Get the arguments and options to build the parameters.
        var locations = context.ParseResult.GetValueForOption<string[]>(_requiredLocation);
        var startTime = context.ParseResult.GetValueForOption<DateTimeOffset?>(_startTime);
        var endTime = context.ParseResult.GetValueForOption<DateTimeOffset?>(_endTime);
        
        var parameters = new CarbonAwareParametersBaseDTO() { 
            MultipleLocations = locations,
            Start = startTime,
            End = endTime
        };

        // Call the aggregator.
        var results = await aggregator.GetEmissionsDataAsync(parameters);

        context.Console.WriteLine(JsonSerializer.Serialize(results));
        context.ExitCode = 0;
    }
}