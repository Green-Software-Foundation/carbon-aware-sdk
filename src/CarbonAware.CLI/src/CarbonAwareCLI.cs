
using CarbonAware.CLI.Options;
using CommandLine.Text;
using Microsoft.Extensions.Logging;
using CarbonAware.Aggregators.CarbonAware;

namespace CarbonAware.CLI;

public class CarbonAwareCLI
{
    public CarbonAwareCLIState _state { get; set; } = new CarbonAwareCLIState();
    
    /// <summary>
    /// Indicates if the command line arguments have been parsed successfully 
    /// </summary>
    public bool Parsed { get; private set; } = false;
    ICarbonAwareAggregator _aggregator {get; set;}

    private readonly ILogger<CarbonAwareCLI> _logger;

    public CarbonAwareCLI(string[] args, ICarbonAwareAggregator aggregator, ILogger<CarbonAwareCLI> logger)
    {
        this._aggregator = aggregator;
        this._logger = logger;
        
        var parseResult = Parser.Default.ParseArguments<CLIOptions>(args);
        try
        {
            // Parse command line parameters
            parseResult.WithParsed(ValidateCommandLineArguments);
            parseResult.WithNotParsed(errors => ThrowOnParseError(errors, parseResult));
            Parsed = true;
        }
        catch (AggregateException e)
        {
            _logger.LogError("Error: {message}", e.Message);
        }
    }



    /// <summary>
    /// Handles missing messages.  Currently reports the message tag as an aggregate exception.
    /// This method needs updating to add detailed "Missing parameter" messages
    /// </summary>
    /// <param name="errors"></param>
    /// <exception cref="AggregateException"></exception>
    private void ThrowOnParseError(IEnumerable<Error> errors, ParserResult<CLIOptions> parseResult)
    {
        var builder = SentenceBuilder.Create();
        var errorMessages = HelpText.RenderParsingErrorsTextAsLines(parseResult, builder.FormatError, builder.FormatMutuallyExclusiveSetErrors, 1);
        var excList = errorMessages.Select(msg => new ArgumentException(msg)).ToList();
        if (excList.Any())
        {
            throw new AggregateException(excList);
        }
    }

    public async Task<IEnumerable<EmissionsData>> GetEmissions()
    {
        IEnumerable<Location> locations = _state.Locations.Select(loc => new Location(){ Name = loc });

        var parameters = new CarbonAwareParameters(){ MultipleLocations = locations };
        if (_state.Time.HasValue)
            parameters.Start = _state.Time.Value;

        if (_state.ToTime.HasValue)
            parameters.End = _state.ToTime.Value;

        if (_state.Lowest)
        {
            return await _aggregator.GetBestEmissionsDataAsync(parameters);
        }
        else
        {
            return await _aggregator.GetEmissionsDataAsync(parameters);
        }
    }

    private async Task<IEnumerable<EmissionsData>> GetEmissionsDataAsync(CarbonAwareParameters parameters)
    {
        return await _aggregator.GetEmissionsDataAsync(parameters);
    }

    public void OutputEmissionsData(IEnumerable<EmissionsData> emissions)
    {
        var emissionsData = new Dictionary<string, IEnumerable<EmissionsData>>() {
            { "emissionsData", emissions }
        };

        var outputJson = JsonConvert.SerializeObject(emissionsData, Formatting.Indented);
        _logger.LogCritical(outputJson);
        Console.WriteLine(outputJson);
    }

    private void ValidateCommandLineArguments(CLIOptions o)
    {
        // -v --verbose 
        ParseVerbose(o);

        // -t --time --toTime
        ParseTime(o);

        // --lowest
        ParseLowest(o);

        // -c --config
        ParseConfigPath(o);

        // -l --locations
        ParseLocations(o);
    }

    #region Parse Options 

    private void ParseLocations(CLIOptions o)
    {

        _state.Locations.AddRange(o.Location);
    }

    private void ParseLowest(CLIOptions o)
    {
        _state.Lowest = o.Lowest;
    }

    private void ParseVerbose(CLIOptions o)
    {
        if (o.Verbose)
        {
            _state.Verbose = true;
        }
    }

    private void ParseConfigPath(CLIOptions o)
    {
        var configPath = o.ConfigPath;

        if (configPath is not null)
        {
            CheckFileExists(configPath);
            _state.ConfigPath = configPath;
        }
    }

    private static void CheckFileExists(string configPath)
    {
        if (!File.Exists(configPath))
        {
            throw new ArgumentException($"File '{configPath}' could not be found.");
        }
    }

    private void ParseTime(CLIOptions o)
    {
        ParseTimeFromTime(o);
        ParseTimeToTime(o);
    }

    private void ParseTimeFromTime(CLIOptions o)
    {
        if (o.Time is null)
        {
            _state.TimeOption = TimeOptionStates.Time;
            _state.Time = DateTime.Now;
        }
        else if (o.Time is not null)
        {
            _state.TimeOption = TimeOptionStates.Time;
            try
            {
                _state.Time = DateTime.Parse(o.Time);
            }
            catch
            {
                throw new ArgumentException(
                    $"Date and time needs to be in the format 'xxxx-xx-xx'.  Date and time provided was '{o.Time}'.");
            }
        }
    }

    private void ParseTimeToTime(CLIOptions o)
    {
        if (o.ToTime is not null)
        {
            _state.TimeOption = TimeOptionStates.TimeWindow;

            try
            {
                _state.ToTime = DateTime.Parse(o.ToTime);
            }
            catch
            {
                throw new ArgumentException(
                    $"Date and time needs to be in the format 'xxxxx'.  Date and time provided was '{o.ToTime}'.");
            }
        }
    }

    #endregion
}
