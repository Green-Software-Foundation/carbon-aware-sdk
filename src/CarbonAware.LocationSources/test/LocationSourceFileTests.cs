using CarbonAware.LocationSources.Configuration;
using NUnit.Framework;
using System.Reflection;

namespace CarbonAware.LocationSources.Test;

[TestFixture]
public class LocationSourceFileTests
{

    #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private LocationSourceFile _dataSource { get; set; }
    #pragma warning restore CS8618
    private string _assemblyPath => Assembly.GetExecutingAssembly().Location;
    

    [SetUp]
    public void Setup()
    {
        _dataSource = new LocationSourceFile();
    }

    [TestCase("../newfile.json", TestName = "setting parent's dir")]
    [TestCase("~/newfile.json", TestName = "setting user's home dir")]
    [TestCase(null, TestName = "setting null filepath")]
    [TestCase("", TestName = "setting empty filepath")]
    public void SetLocationDataSourceFile_ThrowsArgumentException(string filePath)
    {
        var ex = Assert.Throws<ArgumentException>(() => _dataSource.DataFileLocation = filePath);
        Assert.That(ex!.Message, Contains.Substring("not supported characters"));
    }

    [TestCase("newfile.json", TestName = "same location as base dir")]
    [TestCase("/file1.json", TestName = "setting root dir")]
    [TestCase("another_dir/anotherfile.json", TestName = "subdir under base dir")]
    [TestCase("new-dir 123/Sub_Dir/anotherfile.json", TestName = "subdirs with numbers and upper case chars under base dir")]
    public void SetLocationDataSourceFile_Success(string filePath)
    {
        _dataSource.DataFileLocation = filePath;
        var expected = Path.Combine(Path.GetDirectoryName(_assemblyPath)!, LocationSourceFile.BaseDirectory, filePath);
        Assert.That(_dataSource.DataFileLocation, Is.EqualTo(expected));
    }
}
