using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonAware.WebApi.IntegrationTests;

/// <summary>
/// This interface is used by the Integration Tests to set up data for different data sources
/// Each method corresponds to the dataset needed by a different integration test
/// </summary>
public interface IDataSourceMocker
{
    /// <summary>
    /// This method overrides configuration, service and builder settings in a web app factory
    /// Used to add singletons or change config settings as needed for the datasource
    /// </summary>
    /// <param name="factory">The WebAppFactory passed in that will be overriden/changed</param>
    /// <returns></returns>
    public WebApplicationFactory<Program> OverrideWebAppFactory(WebApplicationFactory<Program> factory);

    /// <summary>
    /// This sets up a data endpoint with certain parameters so that it can be pinged.
    /// </summary>
    /// <param name="start">The Start of the time interval</param>
    /// <param name="end">The end of the time interval</param>
    /// <param name="location">Which location the server is looking at.</param>
    public abstract void SetupDataMock(DateTimeOffset start, DateTimeOffset end, string location);

    /// <summary>
    /// Initializes the DataSourceMocker with clean setup
    /// </summary>
    public abstract void Reset();

    /// <summary>
    /// Resets the DataSourceMocker and removes all stubs
    /// </summary>
    public abstract void Initialize();

    /// <summary>
    /// Disposal method
    /// </summary>
    void Dispose();
}