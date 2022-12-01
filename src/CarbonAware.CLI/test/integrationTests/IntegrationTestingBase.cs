using CarbonAware.DataSources.Configuration;
using CarbonAware.DataSources.Mocks;
using CarbonAware.DataSources.Json.Mocks;
using CarbonAware.DataSources.WattTime.Mocks;
using NUnit.Framework;
using System.CommandLine.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using CarbonAware.DataSources.ElectricityMaps.Mocks;

namespace CarbonAware.CLI.IntegrationTests;

/// <summary>
/// A base class that does all the common setup for the Integration Testing
/// Overrides WebAPI factory by switching out different configurations via _datasource
/// </summary>
public abstract class IntegrationTestingBase
{
    private string _executableName = "caw";
    internal DataSourceType _dataSource;
    internal string? _emissionsDataSourceEnv;
    internal string? _forecastDataSourceEnv;
    protected IDataSourceMocker _dataSourceMocker;
    protected TestConsole _console = new();


#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public IntegrationTestingBase(DataSourceType dataSource)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        _dataSource = dataSource;
        _emissionsDataSourceEnv = Environment.GetEnvironmentVariable("DataSources__EmissionsDataSource");
        _forecastDataSourceEnv = Environment.GetEnvironmentVariable("DataSources__ForecastDataSource");
    }

    protected async Task<int> InvokeCliAsync(string arguments)
    {
        // Initialize process here
        using var proc = new Process();
        proc.StartInfo.FileName = _executableName;
        // add arguments as whole string
        proc.StartInfo.Arguments = arguments;

        // use it to start from testing environment
        proc.StartInfo.UseShellExecute = false;

        // redirect outputs to have it in testing console
        proc.StartInfo.RedirectStandardOutput = true;
        proc.StartInfo.RedirectStandardError = true;

        // set working directory
        proc.StartInfo.WorkingDirectory = Environment.CurrentDirectory;

        // Set the console to use the TestConsole for standard output and error.
        Console.SetOut(_console.Out.CreateTextWriter());
        Console.SetError(_console.Error.CreateTextWriter());

        // start the process
        proc.Start();

        // get output to testing console.
        using var sOutput = proc.StandardOutput;
        Console.Out.WriteLine(await sOutput.ReadToEndAsync());
        await Console.Out.FlushAsync();

        using var sError = proc.StandardError;
        Console.Error.WriteLine(await sError.ReadToEndAsync());
        await Console.Error.FlushAsync();

        await proc.WaitForExitAsync();

        // Kill the process if WaitForExitAsync times out or is cancelled.
        if (!proc.HasExited)
        {
            proc.Kill();
        }

        // reset Console Streams
        var standardOutput = new StreamWriter(Console.OpenStandardOutput());
        var standardError = new StreamWriter(Console.OpenStandardError());
        standardOutput.AutoFlush = true;
        standardError.AutoFlush = true;
        
        Console.SetOut(standardOutput);
        Console.SetError(standardError);

        // return exit code
        return proc.ExitCode;
    }

    [OneTimeSetUp]
    public void Setup()
    {
        //Switch between different data sources as needed
        //Each datasource should have an accompanying DataSourceMocker that will perform setup activities
        switch (_dataSource)
        {
            case DataSourceType.JSON:
                {
                    Environment.SetEnvironmentVariable("DataSources__EmissionsDataSource", "Json");
                    Environment.SetEnvironmentVariable("DataSources__Configurations__Json__Type", "JSON");
                    _dataSourceMocker = new JsonDataSourceMocker();
                    break;
                }
            case DataSourceType.WattTime:
                {
                    Environment.SetEnvironmentVariable("DataSources__EmissionsDataSource", "WattTime");
                    Environment.SetEnvironmentVariable("DataSources__ForecastDataSource", "WattTime");
                    Environment.SetEnvironmentVariable("DataSources__Configurations__WattTime__Type", "WattTime");
                    _dataSourceMocker = new WattTimeDataSourceMocker();
                    break;
                }
            case DataSourceType.ElectricityMaps:
                {
                    Environment.SetEnvironmentVariable("DataSources__ForecastDataSource", "ElectricityMaps");
                    Environment.SetEnvironmentVariable("DataSources__Configurations__ElectricityMaps__Type", "ElectricityMaps");
                    Environment.SetEnvironmentVariable("DataSources__Configurations__ElectricityMaps__APITokenHeader", "token");
                    Environment.SetEnvironmentVariable("DataSources__Configurations__ElectricityMaps__APIToken", "test");

                    _dataSourceMocker = new ElectricityMapsDataSourceMocker();
                    break;
                }
            case DataSourceType.None:
                {
                    throw new NotSupportedException($"DataSourceType {_dataSource.ToString()} not supported");
                }
        }

        string assemblyCodeBase =
                System.Reflection.Assembly.GetExecutingAssembly().Location;

        // Get directory name
        string dirName = Path.GetDirectoryName(assemblyCodeBase) ?? "";

        // remove URL-prefix if it exists
        if (dirName.StartsWith("file:\\"))
            dirName = dirName.Substring(6);

        // set current folder
        Environment.CurrentDirectory = dirName;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            _executableName += ".exe";
    }

    [SetUp]
    public void SetupTests()
    {
        _console = new TestConsole();
        _dataSourceMocker.Initialize();
    }

    [TearDown]
    public void ResetTests()
    {
        _dataSourceMocker.Reset();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _dataSourceMocker.Dispose();
        Environment.SetEnvironmentVariable("DataSources__EmissionsDataSource", _emissionsDataSourceEnv);
        Environment.SetEnvironmentVariable("DataSources__ForecastDataSource", _forecastDataSourceEnv);
    }
}