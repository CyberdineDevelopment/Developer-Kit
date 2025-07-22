# FractalDataWorks Developer Kit

A comprehensive .NET library framework providing foundational abstractions and implementations for building scalable, maintainable enterprise applications.

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg)]()
[![.NET](https://img.shields.io/badge/.NET-10.0--preview-512BD4)]()
[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](LICENSE)

## Overview

The FractalDataWorks Developer Kit is a layered architecture framework that provides:

- **Core abstractions** for services, configuration, validation, and results
- **Service patterns** with built-in validation, logging, and error handling
- **Configuration management** with validation and registry patterns
- **Enhanced messaging** using the EnhancedEnums pattern for type-safe, maintainable messages
- **Extensible architecture** supporting dependency injection, data access, hosting, and tools

## Architecture

The framework follows a progressive layered architecture:

### Layer 0.5 - Core Foundation (No Dependencies)
- **FractalDataWorks.net** - Core abstractions and base types (targets netstandard2.0 for maximum compatibility)
  - `IFractalService` - Base service abstraction
  - `IFractalConfiguration` - Configuration abstraction
  - `IServiceResult` & `FractalResult<T>` - Consistent result pattern
  - `ServiceMessage` - Enhanced enum-based messaging system
  - `IFractalValidator<T>` - Validation abstractions

### Layer 1 - Domain-Specific Abstractions
- **FractalDataWorks.Services** - Service patterns and base implementations
  - `ServiceBase<TConfiguration, TCommand>` - Base service with validation and logging
  - `IConfigurationRegistry<T>` - Configuration management pattern
  - Built-in command validation and error handling
  
- **FractalDataWorks.Configuration** - Configuration providers and patterns
  - `ConfigurationBase<T>` - Self-validating configuration base class
  - `ConfigurationProviderBase` - Provider pattern implementation
  - `ConfigurationSourceBase` - Configuration source abstractions
  
- **FractalDataWorks.Connections** - Data and messaging connection abstractions
  - Connection interfaces for various data sources
  - Retry and resilience patterns
  
- **FractalDataWorks.DependencyInjection** - DI container abstractions
  - Container-agnostic dependency injection patterns
  - Service registration extensions
  
- **FractalDataWorks.Tools** - Common utilities and helpers
  - Extension methods and utility classes
  - Common helper functions
  
- **FractalDataWorks.Hosts** - Web and worker host abstractions
  - Host service abstractions
  - Background service patterns
  
- **FractalDataWorks.Data** - Data abstractions and common types
  - Repository patterns
  - Data access abstractions

## Package Documentation

Each package has its own detailed README with usage examples and API documentation:

### Core Foundation
- [FractalDataWorks.net](src/FractalDataWorks.net/README.md) - Core abstractions and base types

### Layer 1 Packages
- [FractalDataWorks.Services](src/FractalDataWorks.Services/README.md) - Service patterns and base implementations
- [FractalDataWorks.Configuration](src/FractalDataWorks.Configuration/README.md) - Configuration management system
- [FractalDataWorks.Connections](src/FractalDataWorks.Connections/README.md) - Connection abstractions and base implementations
- [FractalDataWorks.Data](src/FractalDataWorks.Data/README.md) - Data access abstractions and entity base classes
- [FractalDataWorks.DependencyInjection](src/FractalDataWorks.DependencyInjection/README.md) - DI container abstractions *(planning phase)*
- [FractalDataWorks.Hosts](src/FractalDataWorks.Hosts/README.md) - Host service abstractions *(planning phase)*
- [FractalDataWorks.Tools](src/FractalDataWorks.Tools/README.md) - Common utilities and helpers *(planning phase)*

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

## Key Features

### Service Pattern
```csharp
public class MyService : ServiceBase<MyConfiguration, MyCommand>
{
    public MyService(ILogger<MyService> logger, IConfigurationRegistry<MyConfiguration> configs)
        : base(logger, configs)
    {
    }

    protected override async Task<FractalResult<TResult>> ExecuteCore<TResult>(MyCommand command)
    {
        // Implementation with automatic validation and error handling
    }
}
```

### Configuration Management
```csharp
public class MyConfiguration : ConfigurationBase<MyConfiguration>
{
    public string ConnectionString { get; set; }
    public int Timeout { get; set; }
    
    protected override FluentValidation.Results.ValidationResult ValidateCore()
    {
        var validator = new MyConfigurationValidator();
        return validator.Validate(this);
    }
}
```

### Enhanced Messaging
```csharp
// Type-safe, discoverable service messages
_logger.LogError(ServiceMessages.InvalidConfiguration.Format("Missing connection string"));
_logger.LogInformation(ServiceMessages.ServiceStarted.Format(ServiceName));

// Messages are strongly-typed with consistent formatting
var message = ServiceMessages.ConnectionFailed;
_logger.LogError(message.Format(retries, errorMessage));
```

### Result Pattern
```csharp
// Consistent error handling across all services
var result = await service.Execute<Customer>(command);
if (result.IsSuccess)
{
    return Ok(result.Value);
}
else
{
    return BadRequest(result.Error);
}
```

## Code Quality

The framework enforces code quality through:

- **Analyzers**: StyleCop, AsyncFixer, Meziantou.Analyzer, Roslynator
- **Threading Analysis**: Microsoft.VisualStudio.Threading.Analyzers
- **XML Documentation**: Required for all public/protected members
- **Testing**: xUnit v3 with parallel execution
- **Coverage**: Coverlet integration for code coverage
- **Build Configurations**: Progressive quality gates from Debug to Release

### Quality Gate Configurations

| Configuration | Warnings as Errors | Analyzers | Code Style | Use Case |
|--------------|-------------------|-----------|------------|----------|
| Debug | No | Disabled | No | Fast development |
| Experimental | No | Minimal | No | Early prototyping |
| Alpha | No | Minimal | No | Initial testing |
| Beta | Yes | Recommended | Yes | Development |
| Preview | Yes | Recommended | Yes | Pre-release |
| Release | Yes | Recommended | Yes | Production |

## License

Apache License 2.0 - see LICENSE file for details.