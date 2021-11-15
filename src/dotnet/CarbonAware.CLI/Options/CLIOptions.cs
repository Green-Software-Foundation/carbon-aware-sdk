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

        [Option('d', "data-file", Required = true, HelpText = "Emmisions Data File.")]
        public string DataFile { get; set; }

    }
}
