# 0011. CD pipeline for release process

## Status

Accepted

## Context

Currently (v1.0 at least), Carbon Aware SDK does not ship any binaries include client library even if release tag is set on GitHub. All of users who want to use the SDK have to build binaries for themselves.

For example, [README.md for Carbon Hack 22](https://github.com/Green-Software-Foundation/carbon-aware-sdk/blob/4eeca4cf95db755adecf8e4efe66d1a49c9a74b6/README.md) guides participants for Hackerthon can try Web API which is [hosted on Azure](https://carbon-aware-api.azurewebsites.net/swagger/index.html), then we don't need to access all of sources of the SDK, however we have to access SDK source to build client library.

Carbon Aware SDK has client generator for some languages in [src/clients](https://github.com/Green-Software-Foundation/carbon-aware-sdk/tree/bbbc5b89805f057142401be169664504f835bf95/src/clients), and discusses to add .NET library as NuGet package in [ADR-0009](https://github.com/Green-Software-Foundation/carbon-aware-sdk/blob/dev/docs/architecture/decisions/0009-sdk-as-a-c%23-client-library.md). It is very useful for all SDK users / developers if both WebAPI and client libraries are shipped as binaries.

## Decision

Ships both WebAPI container and client libraries when new release happens.

Fortunately, we can use [GitHub Packages](https://docs.github.com/en/packages/learn-github-packages/introduction-to-github-packages) for this purpose, and we can integrate it with [`release` event on GitHub Actions](https://docs.github.com/en/actions/using-workflows/events-that-trigger-workflows#release) (GHA).

In our case, we can ship following binaries via GitHub Packages:

* WebAPI container
* Client libraries
    * .NET
    * Java
    * JavaScript

This ADR aims to ship them when new release happens automatically.

Environment-specific problems are unlikely to happen. All of release binaries will be built on GHA, and we can QA in its workflows. If some problems happen, we will investigate source code and/or GHA workflows.

## Green Impact

Neutral

## References

* https://github.com/Green-Software-Foundation/carbon-aware-sdk/discussions/46
* [GitHub Packages](https://github.com/features/packages)
* [GitHub Actions](https://github.com/features/actions)
