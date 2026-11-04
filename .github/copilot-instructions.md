# Copilot Instructions for Carbon Aware SDK

## Project Overview
The Carbon Aware SDK is a .NET solution that helps developers build carbon-aware applications by providing tools and APIs to access carbon intensity data.

## Technology Stack
- **.NET SDK Version**: 10.0.x (as specified in `global.json`)
- **Language**: C#
- **Target Framework**: net10.0
- **Container Images**: mcr.microsoft.com/dotnet/sdk:10.0
- **Documentation**: Docusaurus (Node.js 18+, uses Yarn)

## Development Guidelines

### Git Commit Requirements
- **All commits MUST be signed** using GPG signatures
- Use `git commit -S` when committing changes
- Verify commits are signed before pushing

### Solution Structure
- Main solution located in `src/`
- Web API project: `src/CarbonAware.WebApi/`
- Core library: `src/GSF.CarbonAware/`
- Samples: `samples/`
- Documentation: `casdk-docs/` (Docusaurus)

### Build and Test
- Run tests with: `dotnet test`
- Build with: `dotnet build`
- Restore dependencies: `dotnet restore`

### Documentation (Docusaurus)
- Documentation is in `casdk-docs/` directory
- **All documentation changes must compile without warnings**
- Build docs with: `yarn build` (from casdk-docs directory)
- Install dependencies: `yarn install --frozen-lockfile`
- Requires Node.js 18 or higher
- The CI/CD pipeline includes a `docs-build` job that validates documentation builds

### CI/CD Workflows
- PR checks run on `1-pr.yaml` workflow
- All workflows use .NET 10.0.x SDK
- Docker containers use mcr.microsoft.com/dotnet/sdk:10.0 base image
- Documentation build is validated on every PR

### Code Quality
- CodeQL analysis is enabled
- Code coverage via Codecov
- Markdown linting is enabled

## Important Notes
- The project has migrated from .NET 8 to .NET 10
- Ensure all project files reference net10.0 target framework
- Container health endpoints use port 8080
- Any changes to documentation must be tested and build without warnings