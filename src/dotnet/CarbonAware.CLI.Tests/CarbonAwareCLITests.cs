using NUnit.Framework;
using Moq;
using Microsoft.Extensions.Logging;
using CarbonAware.Plugins.JsonReaderPlugin;
using System;
using System.IO;

namespace CarbonAware.CLI.Tests;

public class CarbonAwareCLITests
{
    ICarbonAware? plugin;

    [SetUp]
    public void Setup()
    {
        var logger = Mock.Of<ILogger<CarbonAwareJsonReaderPlugin>>();
        plugin = new CarbonAwareJsonReaderPlugin(logger);
    }

    [Test]
    public void testWhenLocationIsProvided()
    {
        string[] args = new string[] {"-l", "test"};
        
        var cli = new CarbonAwareCLI(args, plugin);
        
        Assert.AreEqual("test", cli._state.Locations[0]); 
    }

    [Test]
    public void testWhenStartTimeIsProvided()
    {
        string[] args = new string[] {"-l", "test", "-t", "2021-11-11"};
        
        var cli = new CarbonAwareCLI(args, plugin);
        
        Assert.AreEqual("test", cli._state.Locations[0]); 
        Assert.AreEqual(DateTime.Parse("2021-11-11"), cli._state.Time); 
    }

    [Test]
    public void testWhenStartTimeAndEndTimeIsProvided()
    {
        string[] args = new string[] {"-l", "test", "-t", "2021-11-11", "--toTime", "2021-12-12"};
        
        var cli = new CarbonAwareCLI(args, plugin);

        Assert.AreEqual("test", cli._state.Locations[0]); 
        Assert.AreEqual(DateTime.Parse("2021-11-11"), cli._state.Time); 
        Assert.AreEqual(DateTime.Parse("2021-12-12"), cli._state.ToTime); 
    }

    [Test]
    public void testWhenLocationNotProvided()
    {
        string[] args = new string[] {};
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        new CarbonAwareCLI(args, plugin);
        StringAssert.Contains("Required option 'l, location' is missing.", stringWriter.ToString());
    }

    
    [Test]
    public void testWhenInvaidDateProvided()
    {
        string[] args = new string[] {"-l", "test","-t", "invalid"};
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        
        var ex = Assert.Throws<ArgumentException>(() => new CarbonAwareCLI(args, plugin));
        StringAssert.Contains("Date and time needs to be in the format", ex.Message);
    }
}