using System;
using System.Collections.Generic;
using CarbonAware;
using CarbonAwareLogicPluginSample;
using NUnit.Framework;

namespace CarbonAwareLogicPluginSampleTest
{
    public class MockLocations
    {
        public static Location Sydney = new Location(101, 101);
        public static Location Melbourne = new Location(102, 102);
        public static Location Auckland = new Location(103, 103);
    }

    public class MockRatings
    {
        public static double Sydney = 0.9;
        public static double Melbourne = 0.7;
        public static double Auckland = 0.1;
    }

    public class MockCarbonDataService : ICarbonDataService
    {
        private readonly List<EmissionsData> _data = new List<EmissionsData>()
        {
            // Sydney with very high emissions
            new EmissionsData()
            {
                Location = MockLocations.Sydney,
                Rating = MockRatings.Sydney,
                Time = DateTime.Now
            },
            // Melbourne with high emissions
            new EmissionsData()
            {
                Location = MockLocations.Melbourne,
                Rating = MockRatings.Melbourne,
                Time = DateTime.Now
            },
            // Auckland with very low emissions
            new EmissionsData()
            {
                Location = MockLocations.Auckland,
                Rating = MockRatings.Auckland,
                Time = DateTime.Now
            }
        };

        public List<EmissionsData> GetData()
        {
            return _data;
        }
    }

    public class Tests
    {
        private ICarbonAwareCore _plugin;

        [SetUp]
        public void Setup()
        {
            Console.WriteLine("Test Setup");
            _plugin = new CarbonAwareLogicPlugin(new MockCarbonDataService());
        }

        [TearDown]
        public void TearDown()
        {
            Console.WriteLine("Test TearDown");
        }

        [Test]
        public void TestEmissionsDataForLocationByTime()
        {
            var ed = _plugin.GetEmissionsDataForLocationByTime(MockLocations.Sydney, DateTime.Now);
            Assert.AreEqual(ed.Rating, MockRatings.Sydney);
        }


        [Test]
        public void TestEmissionsDataForLocationsByTime()
        {
            var locations = new List<Location>() {MockLocations.Sydney, MockLocations.Auckland};

            var emissionDataList = _plugin.GetEmissionsDataForLocationsByTime(locations, DateTime.Now);
            
            Assert.AreEqual(emissionDataList.Count, 2);
            Assert.AreEqual(emissionDataList[0].Rating, MockRatings.Sydney);
            Assert.AreEqual(emissionDataList[1].Rating, MockRatings.Auckland);
        }

        [Test]
        public void TestEmissionsDataForBestLocationByTime()
        {
            var locations1 = new List<Location>() { MockLocations.Sydney, MockLocations.Melbourne };
            var locations2 = new List<Location>() { MockLocations.Sydney, MockLocations.Auckland, MockLocations.Melbourne };

            var ed = _plugin.GetBestEmissionsDataForLocationsByTime(locations1, DateTime.Now);
            Assert.AreEqual(ed.Location, MockLocations.Melbourne);

            ed = _plugin.GetBestEmissionsDataForLocationsByTime(locations2, DateTime.Now);
            Assert.AreEqual(ed.Location, MockLocations.Auckland);
        }
    }
}