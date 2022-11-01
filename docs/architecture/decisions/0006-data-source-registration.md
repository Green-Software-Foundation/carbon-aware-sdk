# 6. Data Source Registration

## Status

Accepted

## Date

2022-11-1

## Context

Currently, data sources must be hardcoded into a separate `CarbonAware.DataSources.Registration` project to be configured with the existing dependency injection methods.  This means that data source developers must plumb their code across multiple projects.  It forces external developers who want to use the SDK as-is, but with a custom data source, to copy and modify the entire codebase to wire in their custom data source.  Finally, it adds unnecessary bloat by requiring every possible data source to be included in all release builds.

## Decision

Consumers declare which data sources they want to include in the project's .csproj file. Operators can reference them via the configuration and they will be set up.

## Consequences

### Usability

- Data source developers within this project can make a full contribution without knowledge of other projects.
- External developers still need to copy the project to access the interfaces, but similarly require less knowledge to create, and this paves the way for future enhancements if components are released as public packages.
- Composable data sources reduces build times and artifact sizes.

### Implementation

A single `ServiceCollectionExtension` class extension can be provided to use the configuration & assembly to discover classes which implement data source interfaces. This approach to using [pluggable interfaces](https://learn.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support) follows existing .NET best practices.

The classes would be responsible for their own configuration and dependencies. This means that configuration logic would need to move from its existing `ServiceCollectionExtensions` location to a static class on the data source.

#### Example

```csharp
// Get the config
var config = configuration
    .GetSection(CarbonAwareVariablesConfiguration.Key)
    .Get<CarbonAwareVariablesConfiguration>();

// Load the assembly for the configured 'CarbonIntensityDataSource'.
// EG 'WattTime'
var assembly = Assembly.Load($"CarbonAware.DataSources.{config.CarbonIntensityDataSource}");

// Get the classes in the CarbonAware.DataSources.WattTime project 
// that implement the ICarbonIntensityDataSource interface.
// Pick the first, because we only expect one right now.
var carbonIntensityDataSourceType = assembly.GetTypes()  
    .Where(type => typeof(ICarbonIntensityDataSource).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
    .First();

// Call static configuration method on the data source to allow it 
// to configure itself and its dependencies. 
MethodInfo configureMethod = typeof(carbonIntensityDataSourceType).GetMethod(
    "ConfigureDI",
    BindingFlags.Static | BindingFlags.Public
);
configureMethod.Invoke(null, services, configuration);
```

```csharp
public class WattTimeDataSource : ICarbonAwareDataSource
{
  public static ConfigureDI(IServiceCollection services, IConfiguration configuration)
  {
    services.ConfigureWattTimeClient(configuration);
    services.TryAddSingleton<ICarbonIntensityDataSource, WattTimeDataSource>();
    services.TryAddSingleton<ILocationSource, AzureLocationSource>();
  }
  // ...
}
```

## Green Impact

Positive

By reducing the size of releases, less energy is required to store and [transmit](https://patterns.greensoftware.foundation/catalog/cloud/reduce-transmitted-data) the data throughout the rest of the SDLC journey.

## Additional Resources

[Plugins with DI](https://jussihaapanen.com/posts/dotnet-core-plugin-dependency-injection/)
