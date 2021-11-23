using CommandLine;
using System;
using System.Collections.Generic;
using CarbonAware;
using Newtonsoft.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

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
