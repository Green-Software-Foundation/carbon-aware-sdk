using CarbonAware;
using CarbonAware.Plugins.BasicJsonPlugin;
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
        private class LocationJsonFile
        {
            public List<Location> Locations { get; set; }
        }

        private State _state { get; set; } = new State();

        public class Options
        {
            [Option('l', "location", Separator = ',', Required = true, HelpText = "The location in latitude/longitude format i.e. \"123.0454,21.4857\"")]
            public List<string> Location { get; set; }

            [Option("lowest", Required = false, HelpText = "Only return the lowest emission result of all matching results.")]
            public bool Lowest { get; set; }

            [Option('t', "time", Required = false, HelpText = "The date and time to get the emissions from.  If no time or time window is provide.")]
            public string Time { get; set; }

            [Option("timeWindowFrom", Required = false, HelpText = "The date and time the start of a time window")]
            public string TimeWindowFrom { get; set; }

            [Option("timeWindowTo", Required = false, HelpText = "The date and time for the end of a time window")]
            public string TimeWindowTo { get; set; }

            [Option('o', "output", Required = false, Default = "console", HelpText = "Output value.  Options: console, json")]
            public string Output { get; set; }

            [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
            public bool Verbose { get; set; }
        }

        private enum TimeOptionStates
        {
            Time,
            TimeWindow
        }

        private enum OutputOptionStates
        {
            Default,
            Json
        }

        private enum LocationOptionStates
        {
            Single,
            File,
            List
        }

        private class State
        {
            public TimeOptionStates TimeOption { get; set; }
            public OutputOptionStates OutputOption { get; set; }

            public List<string> Locations { get; set; } = new List<string>();

            public DateTime Time { get; set; }

            public bool Lowest { get; set; }

            public LocationOptionStates LocationOption { get; set; }
        }


        public CarbonAwareCLI(string[] args)
        {
            var parseResult = Parser.Default.ParseArguments<Options>(args);

            try
            {
                parseResult.WithParsed(ValidateCommandLineArguments);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine("Error: Invalid arguments.");
                Console.WriteLine(e.Message);
                return;
            }

            var ca = new CarbonAwareCore(
                new CarbonAwareBasicDataPlugin(
                    new CarbonAwareStaticJsonDataService("sample-emissions-data.json")));

            List<EmissionsData> foundEmissions = new List<EmissionsData>();

            foreach (var loc in _state.Locations)
            {
                var emissions = ca.GetEmissionsDataForLocationByTime(loc, _state.Time);
                if (emissions != EmissionsData.None)
                {
                    foundEmissions.Add(emissions);
                }
            }

            // This long term should move into the plugin, vs doing it here and
            // assuming it's making the best decision
            if (_state.Lowest && foundEmissions.Count > 0)
            {
                var min = foundEmissions.Min(ed => ed.Rating);
                foundEmissions = foundEmissions.Where(ed => ed.Rating == min).ToList();
            }

            OutputEmissionsData(foundEmissions);
        }

        private void OutputEmissionsData(List<EmissionsData> emissions)
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

        private void ValidateCommandLineArguments(Options o)
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

        private void ParseLowest(Options o)
        {
            _state.Lowest = o.Lowest;
        }

        private static void ParseVerbose(Options o)
        {
            if (o.Verbose)
            {
                var jsonText = JsonConvert.SerializeObject(o);
            }
        }

        private void ParseLocation(Options o)
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

        private void ParseTime(Options o)
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
                catch (FormatException e)
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

        private void ParseOutput(Options o)
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
    }
}
