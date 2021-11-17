using CarbonAware;
using CarbonAware.Plugins.BasicJsonPlugin;
using CarbonAwareCLI.Options;
using CommandLine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace CarbonAwareCLI
{
    public class CarbonAwareCLI
    {
        private CarbonAwareCLIState _state { get; set; } = new CarbonAwareCLIState();
        private CarbonAwareStaticJsonDataService _staticDataService;
        private CarbonAwareCore _carbonAwareCore;

        /// <summary>
        /// Indicates if the command line arguments have been parsed successfully 
        /// </summary>
        public bool Parsed { get; private set; } = false;

        public CarbonAwareCLI(string[] args)
        {
            var parseResult = Parser.Default.ParseArguments<CLIOptions>(args);

            try
            {
                parseResult.WithParsed(ValidateCommandLineArguments);
                Parsed = true;

                _staticDataService = new CarbonAwareStaticJsonDataService(_state.DataFile);
                _carbonAwareCore = new CarbonAwareCore(new CarbonAwareBasicDataPlugin(_staticDataService));
            }
            catch (ArgumentException e)
            {
                Console.WriteLine("Error: Invalid arguments.");
                Console.WriteLine(e.Message);
            }
        }

        public List<EmissionsData> GetEmissions()
        {
            List<EmissionsData> foundEmissions = new List<EmissionsData>();

            if (_state.Lowest)
            {
                var emissions = _carbonAwareCore.GetBestEmissionsDataForLocationsByTime(_state.Locations, _state.Time, _state.ToTime);
                if (emissions != EmissionsData.None)
                {
                    foundEmissions.Add(emissions);
                }
            }
            else
            {
                foundEmissions = _carbonAwareCore.GetEmissionsDataForLocationsByTime(_state.Locations, _state.Time, _state.ToTime);
            }
            return foundEmissions;
        }

        public void OutputEmissionsData(List<EmissionsData> emissions)
        {
            switch (_state.OutputOption)
            {
                case OutputOptionStates.Default:
                    {
                        foreach (var e in emissions)
                        { 
                            Console.WriteLine(
                                $"Emissions for {e.Location} at {e.Time}: {e.Rating}");
                        }

                        break;
                    }
                case OutputOptionStates.Json:
                    {
                        Console.WriteLine($"{JsonConvert.SerializeObject(emissions, Formatting.Indented)}");
                        break;
                    }
            }
        }

        private void ValidateCommandLineArguments(CLIOptions o)
        {
            // -v --verbose 
            ParseVerbose(o);

            // -t --time --toTime
            ParseTime(o);

            // --lowest
            ParseLowest(o);

            // -o --output 
            ParseOutput(o);

            // -d --dafa-file
            ParseDataFile(o);

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

        private void ParseDataFile(CLIOptions o)
        {
            if (o.DataFile is not null)
            {
                if (!File.Exists(o.DataFile))
                {
                    throw new ArgumentException($"File '{o.DataFile}' could not be found.");
                }
                _state.DataFile = o.DataFile;  
            }
        }

        private void ParseTime(CLIOptions o)
        {
            // Validate time arguments
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

        private void ParseOutput(CLIOptions o)
        {
            if (o.Output is not null)
            {
                switch (o.Output)
                {
                    case "json":
                        {
                            _state.OutputOption = OutputOptionStates.Json;
                            break;
                        }
                    case "console":
                        {
                            _state.OutputOption = OutputOptionStates.Default;
                            break;
                        }
                    default:
                        {
                            throw new ArgumentException(
                                $"Error: '{o.Output}' is an invalid output option.  Valid options are 'json' and 'console'.");
                        }
                }
            }
        }
        #endregion
    }
}
