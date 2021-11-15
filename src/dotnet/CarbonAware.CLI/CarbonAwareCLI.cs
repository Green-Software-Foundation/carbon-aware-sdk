using CarbonAware;
using CarbonAware.Plugins.BasicJsonPlugin;
using CarbonAwareCLI.Options;
using CommandLine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CarbonAwareCLI
{
    public class CarbonAwareCLI
    {
        private CarbonAwareCLIState _state { get; set; } = new CarbonAwareCLIState();
        public bool Parsed { get; private set; } = false;

        public CarbonAwareCLI(string[] args)
        {
            var parseResult = Parser.Default.ParseArguments<CLIOptions>(args);

            try
            {
                parseResult.WithParsed(ValidateCommandLineArguments);
                Parsed = true;
            }
            catch (ArgumentException e)
            {
                Console.WriteLine("Error: Invalid arguments.");
                Console.WriteLine(e.Message);
            }
        }

        public List<EmissionsData> GetEmissions()
        { 
            var ca = new CarbonAwareCore(
                new CarbonAwareBasicDataPlugin(
                    new CarbonAwareStaticJsonDataService("data-files\\sample-emissions-data.json")));

            List<EmissionsData> foundEmissions = new List<EmissionsData>();

            if (_state.Lowest)
            {
                var emissions = ca.GetBestEmissionsDataForLocationsByTime(_state.Locations, _state.Time);
                if (emissions != EmissionsData.None)
                {
                    foundEmissions.Add(emissions);
                }
            }
            else
            {
                foundEmissions = ca.GetEmissionsDataForLocationsByTime(_state.Locations, _state.Time);
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
                        Console.WriteLine($"{JsonConvert.SerializeObject(emissions)}");
                        break;
                    }
            }
        }

        private void ValidateCommandLineArguments(CLIOptions o)
        {
            // -v --verbose 
            ParseVerbose(o);

            // -l --location --locations
            ParseLocation(o);

            // -t --time --timeWindowTo --timeWindowFrom
            ParseTime(o);

            // --lowest
            ParseLowest(o);

            // -o --output 
            ParseOutput(o);
        }

        #region Parse Options 

        private void ParseLowest(CLIOptions o)
        {
            _state.Lowest = o.Lowest;
        }

        private static void ParseVerbose(CLIOptions o)
        {
            if (o.Verbose)
            {
                var jsonText = JsonConvert.SerializeObject(o);
            }
        }

        private void ParseLocation(CLIOptions o)
        {
            if (o.Location is null)
            {
                // Should never be null
            }
            else
            {
                // THIS IS NOW A LIST

                //try
                //{
                //    var locationStrings = JsonConvert.DeserializeObject<List<string>>(o.Location);
                //    foreach (var ls in locationStrings)
                //    {
                //        _state.Locations.Add(ls);
                //        _state.LocationOption = LocationOptionStates.List;
                //    }
                //}
                //catch (Exception e)
                //{
                //    _state.Locations.Add(o.Location);
                //    _state.LocationOption = LocationOptionStates.Single;
                //}
            }
        }

        private void ParseTime(CLIOptions o)
        {
            // Validate time arguments
            if (o.Time is not null && (o.TimeWindowFrom is not null || o.TimeWindowTo is not null))
            {
                throw new ArgumentException($"-t --time is designed to be used exclusively for a single point in time and must not be used in conjunction with the time window arguments --timeWindowFrom and --timeWindowTo.");
            }

            if (o.Time is null && o.TimeWindowFrom is null && o.TimeWindowTo is null)
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
            else if (o.TimeWindowFrom is not null && o.TimeWindowTo is not null)
            {
                _state.TimeOption = TimeOptionStates.TimeWindow;
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
