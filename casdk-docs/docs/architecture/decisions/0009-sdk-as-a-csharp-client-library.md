# 0009. SDK as a C# Client Library

## Status

Accepted

## Date

2022-11-1

## Context

Currently the SDK can only be used as a runtime (CLI/Web API) and we are
exploring what it would take to turn it into a C# client library. This involves
determining what would be exposed as part of the library and how to call it. We
also include any changes we believe should be made to the current state of the
SDK to better support a library.

## Decision

In order to best support a library, we propose adding a new dotnet project that
lives above the Aggregator to handle access to the core business logic and
configuration management. Along with this, we have noted other issues that we
believe should also be addressed.

**[Must Address]** Creation of a shim that lives on top of the Aggregator and is
exposed in library

- Benefits
  - Continues the consumer tier model (Web API/CLI) wherein the user doesn't
    have access to the aggregator directly but rather via an intermediary layer.
  - Keeps complex logic internal and only exposes top-level requests
  - Enables more tailored, specific documentation about how to use it from a
    consumer perspective, rather than trying to squeeze both general and
    technical documentation onto the aggregator directly.
- Effort - Medium
  - Add a new dotnet project
  - Add robust documentation
  - Add full testing suite

**[Must Address]** Creation of a parameters builder class to shield library
users from directly instantiating the CarbonAwareParameters.

- Benefits
  - Keeps internal DTOs private and does not require user to understand expected
    internal types + parsing.
  - Builder has greater usability; easy to understand and use intermediary to
    instantiate an immutable CarbonAwareParameters.
  - Can design for passing of extra parameters (that a specific data source may
    need) and the internally handle converting to CarbonAwareParameters
- Effort - Low
  - Write a small builder on top of existing class with no new functionality.

**[Should Address]**: Clear access boundaries throughout SDK ala
public/internal/private classes/records

- Benefits
  - Users don't need to onboard onto all complexity of the SDK in order to use,
    they only have to understand the exposed classes.
  - Users aren't calling SDK classes that aren't fully documented and/or don't
    have the guarantee of stability or consistency that the upper level consumer
    tier classes do
  - Users can’t fall into edge cases by calling classes “down the stack” that
    had implicit checks/requirements that were validated higher up
  - Users can't modify objects that are implicitly expected to be immutable by
    the SDK because of the call stack order
- Effort - Medium/Large
  - Would need to do a large scale refactor that may break internal access

**[Future Scope]**: Managed subsystems for Carbon Aware access, configuration,
data source credentials etc.

- Benefits
  - Improves security because Env isn’t the most secure way to handle
    credentials. Could build a robust credential manager.
  - Improves maintainability of the SDK because each sub-system can be worked
    on/improved in isolation without being tied to all the other sub-systems.
  - Flexible/dynamic configurations allow users to make changes live
- Effort - Large

## Consequences

The SDK will be in a state where it can be packaged into binaries for users to
integrate with directly. With the changes, the amount of onboarding needed to
use the SDK library, and the amount of code that needs to be written to call it
should be minimal.

## Green Impact

Neutral
