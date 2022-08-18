using NUnit.Framework;
using Moq;
using System;
using System.IO;
using CarbonAware.Aggregators.CarbonAware;
using Microsoft.Extensions.Logging;
using CarbonAware.Model;
using System.Collections.Generic;

namespace CarbonAware.CLI.Tests;

public class CarbonAwareCLITests
{
    [Test]
    public void ParseCommandLineArguments_SetsLocationWhenProvided()
    {
        string[] args = new string[] {"-l", "test"};
        
        var cli = new CarbonAwareCLI(args, It.IsAny<ICarbonAwareAggregator>(), Mock.Of<ILogger<CarbonAwareCLI>>());
        
        Assert.AreEqual("test", cli._state.Locations[0]); 
    }

    [Test]
    public void ParseCommandLineArguments_SetsStartTimeWhenProvided()
    {
        string[] args = new string[] {"-l", "test", "-t", "2021-11-11"};
        
        var cli = new CarbonAwareCLI(args, It.IsAny<ICarbonAwareAggregator>(), Mock.Of<ILogger<CarbonAwareCLI>>());
        
        Assert.AreEqual("test", cli._state.Locations[0]); 
        Assert.AreEqual(DateTime.Parse("2021-11-11"), cli._state.Time); 
    }

    [Test]
    public void ParseCommandLineArguments_ReturnsJsonWithArrayInside()
    {
        string[] args = new string[] { "-l", "test", "-t", "2021-11-11", "--lowest" };

        EmissionsData exampleEmissionsData = new EmissionsData()
        {
            Time = DateTime.Now + TimeSpan.FromHours(-1),
            Location = "US",
            Rating = 100
        };

        var cli = new CarbonAwareCLI(args, It.IsAny<ICarbonAwareAggregator>(), Mock.Of<ILogger<CarbonAwareCLI>>());

        using (StringWriter writer = new StringWriter())
        {
            // Redirect Console.Out so we can verify what is written
            Console.SetOut(writer);

            cli.OutputEmissionsData(new List<EmissionsData> { exampleEmissionsData, exampleEmissionsData });

            // Verify that output is in a JSON format. For now, just checking the first character
            // TODO: Expand out to full verification (other tests + helper methods)
            string jsonOutput = writer.ToString();
            Assert.IsTrue(jsonOutput.StartsWith("{"));
        }
    }

    [Test]
    public void ParseCommandLineArguments_SetsStartTimeAndEndTimeWhenProvided()
    {
        string[] args = new string[] {"-l", "test", "-t", "2021-11-11", "--toTime", "2021-12-12"};
        
        var cli = new CarbonAwareCLI(args, It.IsAny<ICarbonAwareAggregator>(), Mock.Of<ILogger<CarbonAwareCLI>>());

        Assert.AreEqual("test", cli._state.Locations[0]); 
        Assert.AreEqual(DateTime.Parse("2021-11-11"), cli._state.Time); 
        Assert.AreEqual(DateTime.Parse("2021-12-12"), cli._state.ToTime); 
    }

    [Ignore("Ignore for now due to swallowed error")]
    [Test]
    public void ParseCommandLineArguments_ThrowsErrorWhenLocationNotProvided()
    {
        string[] args = new string[] { };
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        new CarbonAwareCLI(args, It.IsAny<ICarbonAwareAggregator>(), Mock.Of<ILogger<CarbonAwareCLI>>());
        StringAssert.Contains("Required option 'l, location' is missing.", stringWriter.ToString());
    }

    [Test]
    public void ParseCommandLineArguments_ThrowsErrorWhenInvalidDateProvided()
    {
        string[] args = new string[] {"-l", "test","-t", "invalid"};
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        
        var ex = Assert.Throws<ArgumentException>(() => new CarbonAwareCLI(args, It.IsAny<ICarbonAwareAggregator>(), Mock.Of<ILogger<CarbonAwareCLI>>()));

        StringAssert.Contains("Date and time needs to be in the format", ex?.Message);
    }
}