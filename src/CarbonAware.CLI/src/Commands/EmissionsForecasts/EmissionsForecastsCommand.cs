using CarbonAware.CLI.Common;
using CarbonAware.CLI.Model;
using GSF.CarbonAware.Handlers;
using GSF.CarbonAware.Handlers.CarbonAware;
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

    private readonly Dictionary<string, string> _displayNameMap = new Dictionary<string, string>()
    {
        { "MultipleLocations", "--location"},
        { "SingleLocation", "--location"},
        { "Start", "--data-start-at" },
        { "End", "--data-end-at" },
        { "Duration", "--window-size" },
        { "Requested", "--requested-at" },
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
        // Get handler via DI.
        var serviceProvider = context.BindingContext.GetService(typeof(IServiceProvider)) as IServiceProvider ?? throw new NullReferenceException(nameof(IServiceProvider)); 
        var forecastHandler = serviceProvider.GetService(typeof(IForecastHandler)) as IForecastHandler ?? throw new NullReferenceException(nameof(IForecastHandler));

        // Get the arguments and options to build the parameters.
        var locations = context.ParseResult.GetValueForOption<string[]>(_requiredLocation);
        var startTime = context.ParseResult.GetValueForOption<DateTimeOffset?>(_dataStartAt);
        var endTime = context.ParseResult.GetValueForOption<DateTimeOffset?>(_dataEndAt);
        var requestedAt = context.ParseResult.GetValueForOption<DateTimeOffset?>(_requestedAt);
        var duration = context.ParseResult.GetValueForOption<int?>(_windowSize);

        // Call the handler
        var forecastParameters = new CarbonAwareParametersBaseDTO(_displayNameMap)
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
                var forecast = await forecastHandler.GetForecastByDateAsync(forecastParameters.SingleLocation!, forecastParameters.Start, forecastParameters.End, requestedAt, forecastParameters.Duration);
                if (forecast != null)
                {
                    emissionsForecast.Add(EmissionsForecastDTO.FromEmissionsForecast(forecast, forecastParameters.Requested, forecastParameters.Start, forecastParameters.End));
                }
            }
        }
        else
        {
            forecastParameters.MultipleLocations = locations;
            var results = await forecastHandler.GetCurrentForecastAsync(forecastParameters.MultipleLocations!, forecastParameters.Start, forecastParameters.End, forecastParameters.Duration);
            if (results != null)
            {
               emissionsForecast = results.Select(forecast => EmissionsForecastDTO.FromEmissionsForecast(forecast, forecastParameters.Requested, forecastParameters.Start, forecastParameters.End)).ToList();
            }
        }
        var serializedOuput = JsonSerializer.Serialize(emissionsForecast);
        context.Console.WriteLine(serializedOuput);
    }
}
