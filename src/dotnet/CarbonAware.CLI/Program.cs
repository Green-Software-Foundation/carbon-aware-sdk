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

            GenerateDummyAzureData();
        }

        private static void GenerateDummyAwsData()
        {
            var aws = new AwsRegionStaticDataService(@"aws-regions.json");
            var regions = aws.GetRegionData();
            var emData = aws.GenerateDummyData(regions);
            var s = JsonConvert.SerializeObject(emData);
            Console.WriteLine(s);
        }

        private static void GenerateDummyAzureData()
        {
            var aws = new AzureRegionDummyDataGenerator(@"azure-regions.json");
            var regions = aws.GetRegionData();
            var emData = aws.GenerateDummyData(regions);
            var s = JsonConvert.SerializeObject(emData);
            Console.WriteLine(s);
        }
    }
}
