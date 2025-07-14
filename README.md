# FractalDataWorks Developer Kit

Core packages for the FractalDataWorks platform, providing foundational abstractions and implementations for services, connections, configuration, and more.

## Repository Structure

This repository contains the Layer 0.5 and Layer 1 packages as defined in the FractalDataWorks architecture:

### Layer 0.5 - Core Foundation (No Dependencies)
- **FractalDataWorks.net** - Core abstractions and base types (targets netstandard2.0 for maximum compatibility)

### Layer 1 - Base Abstractions
- **FractalDataWorks.Services** - Service abstractions and patterns
- **FractalDataWorks.Connections** - Connection abstractions for data and messaging
- **FractalDataWorks.Configuration** - Configuration abstractions and providers
- **FractalDataWorks.DependencyInjection** - DI abstractions and extensions
- **FractalDataWorks.Tools** - Common tools and utilities
- **FractalDataWorks.Hosts** - Host abstractions for web and worker services
- **FractalDataWorks.Data** - Data abstractions and common types

## Git Workflow

This repository follows a git-flow branching strategy:

1. **master** - Production-ready releases only
2. **develop** - Main development branch
3. **feature/** - Feature branches
4. **beta/** - Beta release branches
5. **release/** - Release candidate branches
6. **experimental/** - Experimental features

### Setting up the Development Branch

After cloning, create the develop branch from master:

```bash
git checkout -b develop
git push -u origin develop
```

### Creating Feature Branches

Always branch from develop:

```bash
git checkout develop
git pull origin develop
git checkout -b feature/your-feature-name
```

## Building and Testing

### Prerequisites
- .NET 10.0 Preview SDK
- Visual Studio 2022 Preview or VS Code

### Build Commands

```bash
# Restore packages
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test

# Pack NuGet packages
dotnet pack
```

### Configuration-Specific Builds

```bash
# Debug build (default)
dotnet build

# Alpha build
dotnet build -c Alpha

# Beta build
dotnet build -c Beta

# Release build
dotnet build -c Release
```

## Package Dependencies

Each Layer 1 package depends on FractalDataWorks.net. Additional dependencies:

- **FractalDataWorks.DependencyInjection** also depends on FractalDataWorks.Configuration
- **FractalDataWorks.Hosts** also depends on FractalDataWorks.Services

## Testing

All projects use xUnit.v3 for testing. Test projects follow the naming convention:
`FractalDataWorks.[Package].Tests`

Run tests with:
```bash
dotnet test
```

## CI/CD

This repository includes both Azure Pipelines and GitHub Actions workflows for CI/CD.

### Azure Pipelines
- Configuration: `azure-pipelines.yml`
- Publishes to Azure Artifacts feed: `dotnet-packages`

### GitHub Actions
- Configuration: `.github/workflows/ci.yml`
- Publishes to GitHub Packages and optionally Azure Artifacts

## Contributing

1. Create a feature branch from develop
2. Make your changes
3. Ensure all tests pass
4. Submit a pull request to develop

## License

MIT License - see LICENSE file for details.