# FractalDataWorks Developer Kit

🚧 **IN PROGRESS** - Enhanced Enum Type Factories implementation in progress

A comprehensive .NET library framework providing foundational abstractions and implementations for building scalable, maintainable enterprise applications.

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg)]()
[![.NET](https://img.shields.io/badge/.NET-10.0--preview-512BD4)]()
[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](LICENSE)

## Overview

The FractalDataWorks Developer Kit is a layered architecture framework that provides:

- **Core abstractions** for services, configuration, validation, and results
- **Service patterns** with built-in validation, Serilog structured logging, and error handling
- **Configuration management** with validation and registry patterns
- **Service and Message architecture** using Enhanced Enums for type-safe, discoverable service types and messages
- **Cross-assembly discovery** with optional source generators for automatic collection generation
- **Extensible architecture** supporting dependency injection, data access, hosting, and tools

## Architecture

The framework follows a progressive layered architecture with clear separation between core abstractions and implementations. All core interfaces reside in Layer 0.5, while implementations are provided in Layer 1 packages.

### Key Architectural Patterns

1. **Universal Data Service**: A single data service handles all data operations through a command pattern with provider-specific implementations
2. **External Connections as Boundaries**: External connections represent the boundary between the framework and external systems
3. **Command Transformation**: Universal commands (LINQ-like) are transformed to provider-specific commands via command builders
4. **Service Factory Pattern**: Services use factories to obtain and manage connections internally

### Layer 0.5 - Core Foundation (No Dependencies)
- **FractalDataWorks.net** - Core abstractions and base types (targets netstandard2.0 for maximum compatibility)
  - `IFdwService` - Base service abstraction
  - `IFdwConfiguration` - Configuration abstraction
  - `IFdwResult` & `FdwResult<T>` - Consistent result pattern
  - `ServiceMessage` - Enhanced enum-based messaging system
  - `IFdwValidator<T>` - Validation abstractions
  - `IServiceFactory` - Service factory abstraction
  - `IConfigurationRegistry` - Configuration registry abstraction
  - `IExternalConnection` - External connection boundary interface
  - `IFdwConnection` - Framework connection wrapper
  - `IDataConnection` - Universal data service abstraction
  - `IFdwDataCommand` - Universal data command interface (Query/Insert/Update/Upsert/Delete)

### Layer 1 - Domain-Specific Implementations
- **FractalDataWorks.Services** - Service patterns, base implementations, and service/message infrastructure
  - `ServiceBase<TConfiguration, TCommand>` - Base service with validation and Serilog structured logging
  - `ServiceTypeBase<T>` - Base class for service type definitions with Enhanced Enum support
  - `MessageBase<T>` - Base class for result messages (moved from separate Messages project) with Enhanced Enum support
  - `ServiceTypeAttribute` - Attribute for marking service types for discovery
  - `MessageAttribute` - Attribute for marking messages for discovery
  - `DataConnection<TDataCommand, TConnection>` - Universal data service implementation
  - Built-in command validation and error handling
  - Comprehensive logging with ServiceBaseLog using source generators and Serilog destructuring
  - All service-related infrastructure consolidated in one package
  
- **FractalDataWorks.Configuration** - Configuration providers and patterns
  - `ConfigurationBase<T>` - Self-validating configuration base class
  - `ConfigurationProviderBase` - Provider pattern implementation
  - `ConfigurationSourceBase` - Configuration source abstractions
  
- **FractalDataWorks.Connections** - External connection implementations
  - `ExternalConnectionBase<TCommandBuilder, TCommand, TConnection, TFactory, TConfig>` - Base for provider-specific connections
  - `ExternalConnectionProvider` - Selects appropriate connection factory
  - Provider implementations (e.g., `MsSqlConnection`, `PostgresConnection`)
  - Command builders that transform universal commands to provider-specific commands
  
- **FractalDataWorks.DependencyInjection** - DI container abstractions
  - Container-agnostic dependency injection patterns
  - Service registration extensions
  
- **FractalDataWorks.Tools** - Common utilities and helpers
  - Extension methods and utility classes
  - Common helper functions
  
- **FractalDataWorks.Hosts** - Web and worker host abstractions
  - Host service abstractions
  - Background service patterns
  
- **FractalDataWorks.Data** - Entity base classes and data patterns
  - Entity base classes with audit fields
  - Soft delete and versioning support
  - Entity validation patterns

## Service and Message Architecture

The framework provides a unified approach to service types and messages using Enhanced Enums. **FractalDataWorks.Services** now contains all service-related infrastructure in one consolidated package:

### Core Components

#### Service Types and Messages (`FractalDataWorks.Services`)
- **ServiceTypeBase&lt;T&gt;** - Base class for defining service types with Enhanced Enum attributes
- **ServiceTypeAttribute** - Marks classes as service types for discovery
- **MessageBase&lt;T&gt;** - Base class for defining result messages (moved from separate Messages project) with Enhanced Enum attributes  
- **MessageAttribute** - Marks classes as messages for discovery
- All service-related infrastructure consolidated in one package

#### Message Purpose
- Messages are for **result values** in `FdwResult<T>`, not for logging
- They provide structured, type-safe error and success messages
- Enhanced Enum support enables compile-time safety and discoverability

### Cross-Assembly Discovery Options

#### Option 1: Automatic Discovery with Source Generator (Recommended)
Add the optional cross-assembly package to automatically generate static collections:

```xml
<!-- Basic services -->
<PackageReference Include="FractalDataWorks.Services" />

<!-- With automatic cross-assembly discovery -->
<PackageReference Include="FractalDataWorks.ServiceTypes.CrossAssembly" />
```

This generates static collections like:
```csharp
// Automatically generated
ServiceTypes.All              // All service types across assemblies
ServiceTypes.GetById(1)       // Get by ID
ServiceTypes.GetByName("...")  // Get by name
Messages.All                  // All messages across assemblies
```

#### Option 2: Manual Collection Creation
Create your own collections without source generators - this approach is always available:

```csharp
public static class MyServiceTypes
{
    public static readonly List<ServiceTypeBase> All = new()
    {
        new EmailServiceType(),
        new SmsServiceType(),
        // ... other service types
    };
}
```

### Architecture Separation

- **EnhancedEnums** - Cross-functional Enhanced Enum base functionality (separate project)
- **Services** - Consolidated service and message patterns using Enhanced Enums
- **ServiceTypes.CrossAssembly** - Optional source generator for automatic service type discovery

#### Key Points:
- **Messages are for result values** in `FdwResult<T>`, not logging
- **EnhancedEnums remains separate** as a cross-functional project 
- **Cross-assembly generators are optional** - manual collection creation always available
- **Services project contains all service-related infrastructure** in one consolidated package

This separation ensures:
- Clean architecture with focused responsibilities
- Optional cross-assembly discovery via separate packages
- EnhancedEnums remains reusable across different domains
- All service/message infrastructure consolidated for easier management

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

    protected override async Task<FdwResult<TResult>> ExecuteCore<TResult>(MyCommand command)
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
// Type-safe, discoverable service messages with Serilog structured logging
_logger.LogError("Invalid configuration: {Error}", ServiceMessages.InvalidConfiguration.Format("Missing connection string"));
_logger.LogInformation("Service started: {ServiceName}", ServiceName);

// Enhanced structured logging with Serilog destructuring
_logger.LogError("Connection failed after {Retries} attempts: {@Error}", 
    retries, new { Message = errorMessage, Timestamp = DateTime.UtcNow });

// Use ServiceBaseLog for comprehensive structured logging
ServiceBaseLog.CommandExecutedWithContext(_logger, command);
ServiceBaseLog.PerformanceMetrics(_logger, new PerformanceMetrics(150.5, 1000, "BatchProcess"));
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

### Universal Data Service Pattern
```csharp
// Universal data service that works with any data source
public class DataConnection<TDataCommand, TConnection> : ServiceBase<TDataCommand>
    where TDataCommand : IFdwDataCommand
{
    private readonly IExternalConnectionProvider _connectionProvider;
    
    protected override async Task<FdwResult<TResult>> ExecuteCore<TResult>(TDataCommand command)
    {
        // Provider selects appropriate connection based on configuration
        var connection = await _connectionProvider.GetConnection(command.ConnectionId);
        return await connection.Execute<TResult>(command);
    }
}

// SQL Server provider implementation
public class MsSqlConnection : ExternalConnectionBase<SqlCommandBuilder, SqlCommand, SqlConnection, SqlConnectionFactory, SqlConfiguration>
{
    protected override SqlCommand BuildCommand(IFdwDataCommand dataCommand)
    {
        // SqlCommandBuilder transforms universal command to SQL
        return CommandBuilder.Build(dataCommand);
    }
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

## Enhanced Enum Type Factories

🚧 **IN PROGRESS** - New pattern for type registration using Enhanced Enums

The Enhanced Enum Type Factories pattern provides compile-time safe, automatically registered factory types for services, connections, and tools. This pattern leverages source generators to create strongly-typed collections with full IntelliSense support.

### Key Benefits

- **Compile-time Safety**: All types are verified at compile time
- **IntelliSense Support**: Full IDE support for generated collections (ServiceTypes.*, ConnectionTypes.*, ToolTypes.*)
- **Automatic DI Registration**: Types are automatically registered with dependency injection
- **Factory Pattern**: Each type acts as a factory for creating instances
- **Discoverability**: Easy discovery of available types through generated static collections

### Quick Example

```csharp
// Define a service type
[EnumOption(1, "EmailNotification", "Email notification service")]
public class EmailNotificationServiceType : ServiceTypeBase<INotificationService, EmailConfiguration>
{
    public override object Create(EmailConfiguration configuration)
    {
        return new EmailNotificationService(configuration);
    }
}

// Use the generated collections
var emailService = ServiceTypes.EmailNotification.Instance;
var allServices = ServiceTypes.All;
var service = ServiceTypes.GetByName("EmailNotification");

// Automatic DI registration
services.AddServiceTypes(Assembly.GetExecutingAssembly());
```

For detailed documentation, see [Enhanced Enum Type Factories Documentation](docs/EnhancedEnumTypeFactories.md).

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