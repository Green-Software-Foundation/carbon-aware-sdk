using CarbonAware.Model;
using CarbonAware.Interfaces;
using Microsoft.Extensions.Configuration;

namespace CarbonAware.Tests
{
    internal class MockDataService : ICarbonAwareStaticDataService
    {
        public EmissionsJsonFile EmissionsFile = new EmissionsJsonFile()
        {
            Date = DateTime.Now.ToString(),
            Emissions = new List<EmissionsData>()
            {
                new EmissionsData()
                {
                    Location = "westus",
                    Rating = 100,
                    Time = DateTime.Now - TimeSpan.FromHours(1)
                },
                new EmissionsData()
                {
                    Location = "eastus",
                    Rating = 50,
                    Time = DateTime.Now - TimeSpan.FromHours(1)
                },
                new EmissionsData()
                {
                    Location = "australiaeast",
                    Rating = 200,
                    Time = DateTime.Now - TimeSpan.FromHours(1)
                }
            }
        };

        public void Configure(IConfigurationSection config)
        {
            // Nothing, all hard coded
        }

        public List<EmissionsData> GetData()
        {
            return EmissionsFile.Emissions;
        }
    }
}
