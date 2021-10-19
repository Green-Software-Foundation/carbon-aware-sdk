using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarbonAware;
using CarbonAwareLogicPluginSample;
using CommandLine;
using Newtonsoft.Json;

namespace CarbonAwareCLI
{
    public class CarbonAwareCLI
    {
        private State _state { get; set; } = new State();

        public class Options
        {
            [Option('l', "location", Required = true, HelpText = "The location in latitude/longitude format i.e. \"123.0454,21.4857\"")]
            public string Location { get; set; }

            [Option('t', "time", Required = false, HelpText = "The date and time to get the emissions from.  If no time or time window is provide.")]
            public string Time { get; set; }

            [Option("timeWindowFrom", Required = false, HelpText = "The date and time the start of a time window")]
            public string TimeWindowFrom { get; set; }

            [Option("timeWindowTo", Required = false, HelpText = "The date and time for the end of a time window")]
            public string TimeWindowTo { get; set; }

            [Option('o', "output", Required = false, Default = "console", HelpText = "Output value.  Options: console, json")]
            public string Output { get; set; }

            [Option('v', "verbose",  Required = false, HelpText = "Set output to verbose messages.")]
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

        private class State
        {
            public TimeOptionStates TimeOption { get; set; }
            public OutputOptionStates OutputOption { get; set; }

            public Location Location { get; set; }
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

            var ca = new CarbonAwareCore(new CarbonAwareLogicPlugin(new CarbonAwareLogicSampleJsonDataService()));

            var emissions = ca.GetEmissionsDataForLocationByTime(_state.Location, DateTime.Now);

            Console.WriteLine($"Emissions for {_state.Location}: {emissions}");
        }

        private void ValidateCommandLineArguments(Options o)
        {
            if (o.Verbose)
            {
                var jsonText = JsonConvert.SerializeObject(o);
                Console.WriteLine($"Verbose output enabled. Current Arguments: {jsonText}" );
            }

            if (o.Location is not null)
            {
                _state.Location = Location.Parse(o.Location);
            }

            // Validate time arguments
            if (o.Time is not null && (o.TimeWindowFrom is not null || o.TimeWindowTo is not null))
            {
                throw new ArgumentException($"-t --time is designed to be used exclusively for a single point in time and must not be used in conjunction with the time window arguments --timeWindowFrom and --timeWindowTo.");
            }
            if (o.Time is not null)
            {
                _state.TimeOption = TimeOptionStates.Time;

            }
            else if (o.TimeWindowFrom is not null && o.TimeWindowTo is not null)
            {
                _state.TimeOption = TimeOptionStates.TimeWindow;
            }

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
                        throw new ArgumentException($"Error: '{o.Output}' is an invalid output option.  Valid options are 'json' and 'console'.");
                    }
                }
            }
        }
    }
}
