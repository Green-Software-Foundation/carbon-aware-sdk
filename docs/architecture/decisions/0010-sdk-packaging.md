# 0010. Create packages for CarbonAware SDK

## Status

Accepted

## Date

2022-11-1

## Context

Current GSF CarbonAware software can't be consumed as
[Nuget](https://www.nuget.org) Packages in case there is an application that
requires to integrate with it. There are scenerarios where the current runtimes
(CLI/WebApi) environments are not allowed to be used since all the functionality
requires to be bundled together. The goal of this proposal is to show that the
current GSF repository can provide a mechanism to generate `Nuget` packages that
can be consumed by any application that wants to integrate with
`GSF CarbonAware`.

## Decision

Having nuget packages available to be consumed by 3rd party application would
help to integrate easily and it would open the possibilities to extend the SDK
more. Currently there are seven (7) `dotnet` projects that can be set to be
packaged with tools like `dotnet pack` or `msbuild -t:pack`.

| CarbonAware Packages                    |
| --------------------------------------- |
| CarbonAware                             |
| CarbonAware.Aggregators                 |
| CarbonAware.DataSources.ElectricityMaps |
| CarbonAware.DataSources.Json            |
| CarbonAware.DataSources.Registration    |
| CarbonAware.DataSources.WattTime        |
| CarbonAware.LocationSources             |
| CarbonAware.Tools.WattTimeClient        |

**[Must Address]** Creation of a minimum set of packages that can be integrate
with a 3rd party application.

- Continues enhancing the SDK to support integration with different type of
  applications.
- Enhance project files (csproj) to incorporate package metadata.
- Enhance project files (csproj) to include resources data files.

**[Should Address]** Creation of a package repository(ies).

- Use a local feed, private feed or nuget.org repositories.
- Setup package metadata to include packageId, version, description, ...

**[Should Address]**: Creation of automated package generation and test.

- Enhance workflows to create packages and upload to a defined Nuget repository
  for public/private consumption.
- Validate package creation that works as expected with a predefined
  application.

## Consequences

- Bundle 3rd party integration.
- Selective application functionality, by picking the required packages to be
  bundled with.
- No need for a runtime environment to integrate.
- No need to clone the repository to have a functional application.

## Green Impact

Neutral

## References

[Package dotnet CLI](https://learn.microsoft.com/en-us/nuget/create-packages/creating-a-package-dotnet-cli)

[Sign Package](https://learn.microsoft.com/en-us/nuget/create-packages/sign-a-package)
