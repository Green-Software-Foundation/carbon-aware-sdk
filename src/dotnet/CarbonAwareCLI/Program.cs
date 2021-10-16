using CommandLine;
using System;

namespace CarbonAwareCLI
{
    class Program
    {
        public class Options
        {
            [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
            public bool Verbose { get; set; }


            [Option('l', "location", Required = true, HelpText = "The location in latitude/longitude format i.e. \"123.0454,21.4857\"")]
            public string Location { get; set; }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(o =>
                   {
                       if (o.Verbose)
                       {
                           Console.WriteLine($"Verbose output enabled. Current Arguments: -v {o.Verbose}");
                           Console.WriteLine("Quick Start Example! App is in Verbose mode!");
                       }
                       else
                       {
                           Console.WriteLine($"Current Arguments: -v {o.Verbose}");
                           Console.WriteLine("Quick Start Example!");
                       }

                       if (o.Location is not null)
                       {
                           Console.WriteLine($"Location: {o.Location}");
                       }
                   });
        }
    }
}
