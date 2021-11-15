using CommandLine;
using System;
using System.Collections.Generic;
using CarbonAware;
using CarbonAware.Plugins.BasicJsonPlugin;
using Newtonsoft.Json;

namespace CarbonAwareCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            var cli = new CarbonAwareCLI(args);
            if (cli.Parsed)
            {
                var emissions = cli.GetEmissions();
                cli.OutputEmissionsData(emissions);
            }
        }
    }
}
