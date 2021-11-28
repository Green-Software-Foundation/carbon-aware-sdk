using CarbonAware.Data;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonAware.Tests
{
    internal class MockDataService : ICarbonAwareStaticDataService
    {
        private List<EmissionsData> _emissionsData = new List<EmissionsData>()
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
        };
        
        public void Configure(IConfigurationSection config)
        {
            // Nothing, all hard coded
        }

        public List<EmissionsData> GetData()
        {
            return _emissionsData;
        }
    }
}
