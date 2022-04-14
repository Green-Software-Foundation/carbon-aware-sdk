using CarbonAware.Plugins.JsonReaderPlugin.Configuration;
using Microsoft.Extensions.Options;

namespace CarbonAwareCLI;

public class CarbonAwareCLI
{


    private CarbonAwareCLIState _state { get; set; } = new CarbonAwareCLIState();
    
    /// <summary>
    /// Indicates if the command line arguments have been parsed successfully 
    /// </summary>
    public bool Parsed { get; private set; } = false;
    ICarbonAware _plugin {get; set;}     
    public CarbonAwareCLI(string[] args, ICarbonAware plugin)
    {
        this._plugin = plugin;

        var parseResult = Parser.Default.ParseArguments<CLIOptions>(args);

        try
        {
            // Parse command line parameters
            parseResult.WithParsed(ValidateCommandLineArguments);
            parseResult.WithNotParsed(ThrowOnParseError);

            // Create the new core using the plugin
            Parsed = true;
        }
        catch (ArgumentException e)
        {
            Console.WriteLine("Error:");
            Console.WriteLine(e.Message);
        }
    }



    /// <summary>
    /// Handles missing messages.  Currently reports the message tag as an argument exception.
    /// This method needs updating to add detailed "Missing parameter" messages
    /// </summary>
    /// <param name="errors"></param>
    /// <exception cref="ArgumentException"></exception>
    private void ThrowOnParseError(IEnumerable<Error> errors)
    {
        var enumerator = errors.GetEnumerator();

        if (enumerator.MoveNext())
        {
            throw new ArgumentException(enumerator.Current.Tag.ToString());
        }

        // TODO: add error message builder such as
        //var builder = SentenceBuilder.Create();
        //var errorMessages = HelpText.RenderParsingErrorsTextAsLines(result, builder.FormatError, builder.FormatMutuallyExclusiveSetErrors, 1);
        //var excList = errorMessages.Select(msg => new ArgumentException(msg)).ToList();
        //if (excList.Any())
        //    throw new AggregateException(excList);
    }

    public async Task<IEnumerable<EmissionsData>> GetEmissions()
    {
        var props = new Dictionary<string, object?>() {
            { CarbonAwareConstants.Locations, _state.Locations.ToList() },
            { CarbonAwareConstants.Start, _state.Time ?? DateTime.Now },
            { CarbonAwareConstants.End, _state.ToTime },
            { CarbonAwareConstants.Best, true }
        };
        return await GetEmissionsDataAsync(props);
        // if (_state.Lowest)
        // {
        //     foundEmissions = _plugin.GetBestEmissionsDataForLocationsByTime(_state.Locations, _state.Time, _state.ToTime);
        // }
        // else
        // {
        //     foundEmissions = _carbonAwareCore.GetEmissionsDataForLocationsByTime(_state.Locations, _state.Time, _state.ToTime);
        // }

        // return foundEmissions;
    }

    private async Task<IEnumerable<EmissionsData>> GetEmissionsDataAsync(Dictionary<string, object?> props)
    {

        IEnumerable<EmissionsData> e = await _plugin.GetEmissionsDataAsync(props);

        return await _plugin.GetEmissionsDataAsync(props);
    }

    public void OutputEmissionsData(Task<IEnumerable<EmissionsData>> emissions)
    {
        var size = Task.FromResult(emissions);

       Console.WriteLine($"{JsonConvert.SerializeObject(emissions.Result, Formatting.Indented)}");
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
                    $"Date and time needs to be in the format 'xxxxx'.  Date and time provided was '{o.Time}'.");
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
