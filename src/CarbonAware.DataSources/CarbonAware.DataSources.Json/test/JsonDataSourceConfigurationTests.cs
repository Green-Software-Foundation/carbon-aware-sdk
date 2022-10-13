using NUnit.Framework;
using CarbonAware.DataSources.Json.Configuration;
using System.Reflection;
using System.IO;
using System;

namespace CarbonAware.DataSources.Json.Tests;

[TestFixture]
public class JsonDataSourceConfigurationTests
{

    #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private JsonDataSourceConfiguration _configuration { get; set; }
    private string AssemblyPath { get; set; }
    #pragma warning restore CS8618
    
    private const string BaseDir = "data-sources/json";

    [SetUp]
    public void Setup()
    {
        _configuration = new JsonDataSourceConfiguration();
        AssemblyPath = Assembly.GetExecutingAssembly().Location;
    }

    [Test]
    public void GetDefaultDataFileLocation_IsNotNull_ExpectedBaseDir()
    {
        Assert.That(_configuration.DataFileLocation, Is.Not.Null);
        var expectedDir = Path.Combine(Path.GetDirectoryName(AssemblyPath)!, BaseDir);
        Assert.That(_configuration.DataFileLocation, Contains.Substring(expectedDir));
    }

    [TestCase("../newfile.json", TestName = "setting parent's dir")]
    [TestCase("~/newfile.json", TestName = "setting user's home dir")]
    [TestCase(null, TestName = "setting null filepath")]
    [TestCase("", TestName = "setting empty filepath")]
    public void SetDataFileLocation_ThrowsArgumentException(string filePath)
    {
        var ex = Assert.Throws<ArgumentException>(() => _configuration.DataFileLocation = filePath);
        Assert.That(ex!.Message, Contains.Substring("not supported characters"));
    }

    [TestCase("newfile.json", TestName = "same location as base dir")]
    [TestCase("/file1.json", TestName = "setting root dir")]
    [TestCase("another_dir/anotherfile.json", TestName = "subdir under base dir")]
    [TestCase("new-dir 123/Sub_Dir/anotherfile.json", TestName = "subdirs with numbers and upper case chars under base dir")]
    public void SetDataFileLocation_Success(string filePath)
    {
        _configuration.DataFileLocation = filePath;
        var expected = Path.Combine(Path.GetDirectoryName(AssemblyPath)!, BaseDir, filePath);
        Assert.That(_configuration.DataFileLocation, Is.EqualTo(expected));
    }
}
