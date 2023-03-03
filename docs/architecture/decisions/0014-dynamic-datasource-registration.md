# 0014. Dynamic Data Source Registration

## Status

Accepted

## Context

Decouple the data sources from the SDK into their own NuGet packages and create a mechanism to allow auto discovery and dynamic registration. This will allow for new data sources to be plugged in with minimum configuration and dependency on the existing code base. 

## Decision

- Decoupling from SDK

Each data source can be built externally with no dependency on the CarbonAware SDK. This helps achieve single responsibility principle and makes the system extensible.

- Increased Maintainability

Currently, the data source registration code is implemented using a switch statement to register the appropriate data source based on configuration. This can be removed completely and only the data source needed can be imported into the SDK.

- Controlled Solution Size

All the data sources are part of the current SDK, even if they are not used at runtime. For e.g., if both **IEmissions** and **IForecast** interfaces are configured to use WattTime, we need not package the JSON and ElectricityMaps data sources, thereby reducing solution’s size.

- Reduction of Security threads

Having decoupled data sources as packages, it allows us to control potential security threads that can be injected into the overall system, by providing the opportunity to certify them.

## Consequences

Be able to have outsourced Data Sources would benefit the overall system in case there is a large number of those by allowing which one should be part of a solution.

## Design Considerations

Currently Data Sources project is consumed by GSF library using Dependency Injection. Each GSF library handlers have references to IForecastDataSource and/or IEmissionsDataSource Data Source interfaces.
To modify this interaction and to make it more dynamic, these are things that required to be considered:

- Public interfaces & Data Records

    Data Sources interfaces and Data Records are internal and available only to certain projects (i.e GSF.CarbonAware). This would require to be changed so consumers can dynamically register those and consume them.

    Effort Level: **Medium**

- Packaging

    Current Data Source project is not allowed to be packaged (NuGet package, see `<IsPackable>` property on one of the Data Source projects), which would require how this is going to be done in terms of, what the package contains, versioning and where to publish it.

    Effort Level: **Medium**

- Load, Register and Instantiate

    Using techniques like **Reflection** and **Assembly Discovery**, it would be possible to load Data Sources assemblies and instantiate classes that implements the interfaces that are available.
    As an example, this could be done via a Data Source class loader

    ```c#
    static Assembly[] GetDataSourceAssemblies()
    {
            var assemblies = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll")
    .Where(x => x.Contains("CarbonAware.DataSource"))
    .Select(x => Assembly.Load(AssemblyName.GetAssemblyName(x)));
            return assemblies.ToArray();
    }
    ```

    Then the consumer could be using Reflection to instantiate the classes interested:

    ```c#
    Type providerType = DataSourceAssembly.GetType("CarbonAware.DataSource.ProviderA");

    var theForecast = Activator.CreateInstance(providerType) as IForecastDataSource;
    theForecast.GetCarbonIntensityForecastAsync(…) // Invoke the interface method.
    ```

    This responsibility should be part of a new GSF library subsystem.

    Effort Level: **Medium**

- Data Source manifest

    Configuration information would be required to be part of the Data Source package, so the GSF handlers can interact with it. Properties like where to locate the assembly, what assembly to load, what classes to interact with (i.e., Builders/Factories) therefore Emissions and Forecast data can be retrieved from GSF handlers. Designing this manifest would help to drive the implementation of the other items.

    Effort Level: **Large**

- GSF Enhancements

    Given the fact the current registration is done using Dependency Injection, GSF library would require to be changed and enhanced to accommodate discovery, how to load the assemblies that are available and that implement Data Sources interfaces. Also understand the configuration that comes from the manifest, in such a way that all the required properties are available.

    Effort Level: **Medium**

- Documentation

    Document how to create 3rd party Data Sources, how to package them and how to configure those based on a Data Source manifest.

    Effort Level: **Medium**

## Green Impact

Positive

## References

[Package dotnet CLI](https://learn.microsoft.com/en-us/nuget/create-packages/creating-a-package-dotnet-cli)

[Sign Package](https://learn.microsoft.com/en-us/nuget/create-packages/sign-a-package)

[Assemblies in .NET](https://learn.microsoft.com/en-us/dotnet/standard/assembly/)
