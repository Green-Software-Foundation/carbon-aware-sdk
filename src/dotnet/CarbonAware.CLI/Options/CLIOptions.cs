using CommandLine;
using System.Collections.Generic;

namespace CarbonAwareCLI.Options
{
    public class CLIOptions
    {
        [Option('l', "location", Separator = ',', Required = true, HelpText = "The location is a comma seperated list of named locations or regions specific to the emissions data provided.")]
        public IEnumerable<string> Location { get; set; }

        [Option("lowest", Required = false, HelpText = "Only return the lowest emission result of all matching results.")]
        public bool Lowest { get; set; }

        [Option('t', "fromTime", Required = false, HelpText = "The desired date and time to retrieve the emissions for.  Defaults to 'now'.")]
        public string Time { get; set; }

        [Option("toTime", Required = false, HelpText = "The date and time to get the emissions to when looking across a time window.")]
        public string ToTime { get; set; }

        [Option('o', "output", Required = false, Default = "json", HelpText = "Output value.  Options: console, json")]
        public string Output { get; set; }

        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }

        [Option('d', "data-file", Required = true, HelpText = "Emmisions Data File.")]
        public string DataFile { get; set; }

    }
}
