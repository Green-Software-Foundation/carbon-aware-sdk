using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.CLI.Common;
using CarbonAware.CLI.Model;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json;

namespace CarbonAware.CLI.Commands.EmissionsForecasts;

class EmissionsForecastsCommand : Command
{
    private readonly Option<string[]> _requiredLocation = CommonOptions.RequiredLocationOption;
    
    private readonly Option<DateTimeOffset?> _dataEndAt = new Option<DateTimeOffset?>(
                new string[] { "--data-end-at", "-e" },
            LocalizableStrings.DataEndAtDecsription)
    {
        Arity = ArgumentArity.ZeroOrOne,
    };
    private readonly Option<DateTimeOffset?> _requestedAt = new Option<DateTimeOffset?>(
                new string[] { "--requested-at", "-r" },
            LocalizableStrings.RequestedAtDescription)
    {
        Arity = ArgumentArity.ZeroOrOne,
    };
    private readonly Option<DateTimeOffset?> _dataStartAt = new Option<DateTimeOffset?>(
            new string[] { "--data-start-at", "-s" },
            LocalizableStrings.DataStartAtDescription)
    {
        Arity = ArgumentArity.ZeroOrOne,
    };
    private readonly Option<int?> _windowSize = new Option<int?>(
                new string[] { "--window-size", "-w" },
            LocalizableStrings.WindowSizeDescription)
    {
        Arity = ArgumentArity.ZeroOrOne,
    };

    public EmissionsForecastsCommand() : base("emissions-forecasts", LocalizableStrings.EmissionsForecastsCommandDescription)
    {
        AddOption(_requiredLocation);
        AddOption(_dataStartAt);
        AddOption(_dataEndAt);
        AddOption(_windowSize);
        AddOption(_requestedAt);

        this.SetHandler(this.Run);
    }

    internal async Task Run(InvocationContext context)
    {
        // Get aggregator via DI.
        var serviceProvider = context.BindingContext.GetService(typeof(IServiceProvider)) as IServiceProvider ?? throw new NullReferenceException(nameof(IServiceProvider)); 
        var aggregator = serviceProvider.GetService(typeof(ICarbonAwareAggregator)) as ICarbonAwareAggregator ?? throw new NullReferenceException(nameof(ICarbonAwareAggregator));

        // Get the arguments and options to build the parameters.
        var locations = context.ParseResult.GetValueForOption<string[]>(_requiredLocation);
        var startTime = context.ParseResult.GetValueForOption<DateTimeOffset?>(_dataStartAt);
        var endTime = context.ParseResult.GetValueForOption<DateTimeOffset?>(_dataEndAt);
        var requestedAt = context.ParseResult.GetValueForOption<DateTimeOffset?>(_requestedAt);
        var duration = context.ParseResult.GetValueForOption<int?>(_windowSize);

        // Call the aggregator
        var forecastParameters = new CarbonAwareParametersBaseDTO()
        {
            Start = startTime,
            End = endTime,
            Duration = duration,
            Requested = requestedAt
    };

        List<EmissionsForecastDTO> emissionsForecast = new();

        // If requestedAt is not provided, fetch the current forecast
        if (requestedAt != null)
        {
            foreach (var location in locations!)
            {
                forecastParameters.SingleLocation = location;
                var forecast = await aggregator.GetForecastDataAsync(forecastParameters);
                if (forecast != null)
                {
                    emissionsForecast.Add((EmissionsForecastDTO)forecast);
                }
            }
        }
        else
        {
            forecastParameters.MultipleLocations = locations;
            var results = await aggregator.GetCurrentForecastDataAsync(forecastParameters);
            if (results != null)
            {
               emissionsForecast = results.Select(forecast => (EmissionsForecastDTO)forecast).ToList();
            }
        }
        var serializedOuput = JsonSerializer.Serialize(emissionsForecast);
        context.Console.WriteLine(serializedOuput);
    }
}
