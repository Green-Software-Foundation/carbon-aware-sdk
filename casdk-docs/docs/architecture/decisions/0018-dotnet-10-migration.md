# 18. Migration to .NET 10

## Status

Proposed

## Context

The Carbon Aware SDK is currently built on .NET 8.0, which was released in November 2023. .NET 10 was released in November 2024 as the next Long Term Support (LTS) version, offering:

- **Long Term Support**: 3 years of support (until November 2027)
- **Performance improvements**: Enhanced JIT compilation, better garbage collection
- **Security updates**: Latest security patches and vulnerability fixes
- **New language features**: C# 13 support
- **Modern tooling**: Updated SDK and runtime capabilities

### Migration Research

A comprehensive review of [Microsoft's .NET 10 breaking changes documentation](https://learn.microsoft.com/en-us/dotnet/core/compatibility/10.0) and community migration guides revealed several critical breaking changes affecting our codebase:

#### 1. ASP.NET Core Model Binding Changes

**Breaking Change**: In .NET 10, when `<Nullable>enable</Nullable>` is configured, ASP.NET Core now treats non-nullable reference types as **implicitly required** for model binding validation.

**Impact on Carbon Aware SDK**:
- 97 integration tests failing in `CarbonAware.WebApi.IntegrationTests`
- API endpoints returning `500 Internal Server Error` instead of `400 Bad Request` when required query parameters are missing
- Tests expecting `BadRequest` responses now receive `InternalServerError`

**Root Cause**:
- Our WebAPI uses `[FromQuery]` parameters with `SwaggerParameter(Required = true)` annotations
- Properties are declared as nullable (`string[]?`) but the validation pipeline now throws exceptions when these "required" parameters are missing
- The exception is unhandled during model binding, resulting in 500 errors instead of proper validation responses

**Example affected code**:
```csharp
[FromQuery(Name = "location"), SwaggerParameter(Required = true)]
public override string[]? MultipleLocations { get; set; }
```

#### 2. Validation API Namespace Changes

The validation APIs have moved to `Microsoft.Extensions.Validation` package for broader usage beyond HTTP scenarios. This doesn't immediately break our code but may affect future extensibility.

#### 3. Container Base Image Updates

Docker images need updating:
- `mcr.microsoft.com/dotnet/sdk:8.0` → `mcr.microsoft.com/dotnet/sdk:10.0`
- `mcr.microsoft.com/dotnet/aspnet:8.0` → `mcr.microsoft.com/dotnet/aspnet:10.0`
- `mcr.microsoft.com/dotnet/runtime:8.0` → `mcr.microsoft.com/dotnet/runtime:10.0`
- Azure Functions: `dotnet-isolated8.0` → `dotnet-isolated10.0`

### Migration Work Completed

The following changes have been implemented on branch `feature/dotnet-10-upgrade`:

1. **Core Framework**: Updated `global.json` from SDK 8.0.201 → 10.0.101
2. **Projects**: Updated all 27 `.csproj` files from `net8.0` → `net10.0`
3. **Docker**: Updated 4 Dockerfiles to use .NET 10 base images
4. **CI/CD**: Updated GitHub Actions workflows to use .NET 10 SDK
5. **Configuration**: Updated VS Code launch.json, package paths, and client generation scripts

**Results**:
- ✅ Build: Successful
- ✅ Unit Tests: All 344 tests pass
- ❌ Integration Tests: 97 tests fail (WebAPI validation behavior change)

## Decision

We will **migrate to .NET 10** with the following approach to address the breaking changes:

### Option Chosen: Update Model Validation Behavior (Option 2)

After evaluating two options:

**Option 1: Suppress Implicit Required Behavior (Quick Fix)**
```csharp
builder.Services.AddControllers(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});
```
- ✅ Quick fix, minimal code changes
- ❌ Loses .NET 10's improved type safety benefits
- ❌ Goes against framework's recommended practices
- ❌ May cause confusion for future contributors

**Option 2: Embrace .NET 10's Model Validation (Chosen)**
- ✅ Aligns with .NET 10 best practices
- ✅ Leverages improved type safety
- ✅ Future-proof for long-term maintenance
- ❌ Requires more careful review of parameter models
- ❌ May require test updates

### Implementation Plan

1. **Review and Update Parameter Models**:
   - Audit all DTO classes with `[FromQuery]` parameters
   - Ensure nullable annotations correctly reflect optional vs required parameters
   - Use `string?` for truly optional parameters
   - Use `string` for required parameters (validated by framework)
   - Consider removing redundant `SwaggerParameter(Required = true)` where type system now handles it

2. **Update Exception Handling**:
   - Review `HttpResponseExceptionFilter` to properly handle model validation exceptions
   - Ensure validation failures return proper `400 Bad Request` with validation details
   - Add proper error response formatting for validation errors

3. **Update Integration Tests**:
   - Verify test expectations align with new validation behavior
   - Ensure error response assertions are correct
   - Add tests for new validation scenarios if needed

4. **Update Documentation**:
   - Document the parameter nullability conventions
   - Update API documentation to reflect validation behavior
   - Add migration notes for future .NET upgrades

### Migration Checklist

- [x] Update global.json SDK version
- [x] Update all .csproj TargetFramework to net10.0
- [x] Update Dockerfiles to .NET 10 images
- [x] Update CI/CD workflows to .NET 10 SDK
- [x] Update VS Code configurations
- [x] Update package and client generation scripts
- [ ] Fix model validation behavior (Option 2 implementation)
- [ ] Update HttpResponseExceptionFilter for validation errors
- [ ] Review and update all DTO parameter models
- [ ] Fix integration tests
- [ ] Update API documentation
- [ ] Create migration guide for contributors

## Consequences

### Positive

- **Long Term Support**: 3 years of support ensures stability and security updates
- **Performance**: Benefit from .NET 10 runtime and JIT improvements
- **Type Safety**: Improved nullable reference type handling prevents bugs at compile time
- **Security**: Access to latest security patches and vulnerability fixes
- **Modern Features**: Can leverage C# 13 and latest framework capabilities
- **Community Support**: Active community support for LTS version

### Negative

- **Breaking Changes**: Requires careful handling of model validation changes
- **Testing Effort**: 97 integration tests need review and potential updates
- **Learning Curve**: Team needs to understand new validation behavior
- **Temporary Disruption**: Migration work delays other feature development
- **Documentation Burden**: Need to document new patterns and migration notes

### Neutral

- **Container Images**: Updated images have similar size and performance characteristics
- **Build Times**: No significant change in build/test times observed
- **Development Experience**: Similar developer experience with minor adjustments

### Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| API behavior changes break clients | High | Thorough testing, clear release notes, semantic versioning |
| Integration test failures mask real issues | Medium | Systematic review of each failing test |
| Team unfamiliarity with new patterns | Medium | Documentation, code review, knowledge sharing |
| Regression in production | High | Staged rollout, comprehensive testing, monitoring |

### Timeline

- **Phase 1**: Framework upgrade (Complete)
- **Phase 2**: Model validation fixes (In Progress) - ~1-2 days
- **Phase 3**: Testing and validation (Pending) - ~1-2 days  
- **Phase 4**: Documentation and PR review (Pending) - ~1 day
- **Total Estimated Effort**: 3-5 days

## Green Impact

**Neutral to Slightly Positive**

The migration to .NET 10 has minimal direct environmental impact:

- **CPU Intensity**: Neutral to slightly positive. .NET 10 includes minor performance improvements in JIT compilation and garbage collection that may reduce CPU cycles for the same workload.

- **Memory Usage**: Neutral. .NET 10's GC improvements may result in slightly better memory utilization patterns.

- **Carbon Intensity**: Neutral. The application's primary purpose (measuring and reducing carbon emissions) remains unchanged, and any performance improvements are marginal.

- **Hardware Requirements**: Neutral. .NET 10 has similar hardware requirements to .NET 8.

- **Container Size**: Neutral. Updated base images have comparable sizes.

- **Long-term Sustainability**: Positive. By staying current with LTS releases, we ensure the project remains maintainable and secure, reducing technical debt that could require more resource-intensive rewrites in the future.

**Conclusion**: While not a green-focused ADR, the migration supports long-term sustainability of the Carbon Aware SDK project without negative environmental impact.

## References

- [.NET 10 Breaking Changes - Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/core/compatibility/10.0)
- [.NET 10 Release Notes - Microsoft](https://github.com/dotnet/core/tree/main/release-notes/10.0)
- [ASP.NET Core Model Validation in .NET 10](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-10.0)
- [Required Properties and Nullable Reference Types](https://www.damirscorner.com/blog/posts/20241220-RequiredPropertiesAndNullableReferenceTypes.html)
- [Migrating to Swashbuckle.AspNetCore v10](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/master/docs/migrating-to-v10.md)
- [.NET 10 Migration Guide - InfoQ](https://www.infoq.com/news/2025/12/asp-net-core-10-release/)

## Related ADRs

- [ADR-0001: Record Architecture Decisions](0001-record-architecture-decisions.md)

## Author

- Date: 2024-12-15
- Migration Branch: `feature/dotnet-10-upgrade`
- Commits: 3 (Framework upgrade, Docker/CI updates, Config updates)
