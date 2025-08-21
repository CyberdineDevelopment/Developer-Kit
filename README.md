# FractalDataWorks Developer Kit

✅ **STABLE** - Complete implementation with Enhanced Enum Type Factories, service discovery, and cross-assembly support

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

#### FractalDataWorks.Configuration.Abstractions
- `IFdwConfiguration` - Configuration abstraction
- `IFdwConfigurationProvider` - Configuration provider interface
- `IFdwConfigurationSource` - Configuration source interface
- `IConfigurationRegistry` - Configuration registry abstraction

#### FractalDataWorks.Services.Abstractions
- `IFdwService` - Base service abstraction
- `ICommand` - Universal command interface
- `ICommandBuilder` - Command builder abstraction
- `ICommandResult` - Command result interface
- `IFdwValidator<T>` - Validation abstractions
- `IServiceFactory` - Service factory abstraction
- `IToolFactory` - Tool factory abstraction
- `IDataCommand` - Data command interface

#### FractalDataWorks.Results
- `IFdwResult` & `FdwResult<T>` - Consistent result pattern
- `IGenericResult` - Generic result interface

#### FractalDataWorks.Messages
- `IFdwMessage` - Message interface with severity levels

### Layer 1 - Domain-Specific Implementations

#### FractalDataWorks.Services
Service patterns, base implementations, and service/message infrastructure:
- `ServiceBase<TCommand, TConfiguration, TService>` - Base service with validation and structured logging
- `ServiceTypeBase<T>` - Base class for service type definitions with Enhanced Enum support
- `ServiceFactoryBase` - Base factory for service creation
- `ServiceTypeProviderBase` - Provider for service type management
- Built-in command validation and error handling
- Comprehensive logging with ServiceBaseLog using source generators

#### FractalDataWorks.Configuration
Configuration providers and patterns:
- `ConfigurationBase<T>` - Self-validating configuration base class (deprecated, use FdwConfigurationBase)
- `FdwConfigurationBase<T>` - New configuration base with FluentValidation.Results.ValidationResult
- `ConfigurationProviderBase` - Provider pattern implementation
- `ConfigurationSourceBase` - Configuration source abstractions
- JsonConfigurationSource for JSON-based configuration

#### FractalDataWorks.Services.DataProvider.Abstractions
Data provider abstractions for universal data operations:
- `DataCommandBase` - Base class for data commands
- `QueryCommand`, `InsertCommand`, `UpdateCommand`, `DeleteCommand`, `UpsertCommand` - Specific command types
- `BulkInsertCommand`, `BulkUpsertCommand`, `CountCommand`, `ExistsCommand` - Additional command types
- `DataStoreConfiguration` - Configuration for external data sources
- `IExternalDataConnection` - Interface for external data connections

#### FractalDataWorks.Services.DataProvider  
Data provider service implementations:
- `DataProviderService` - Universal data service implementation
- `ExternalDataConnectionProvider` - Provider for external data connections
- Enhanced enum support for data commands

#### FractalDataWorks.Services.ExternalConnections.Abstractions
External connection abstractions:
- `IExternalConnection` - Interface for external connections
- `IExternalConnectionFactory` - Factory interface for connections
- `IExternalConnectionService` - Service interface for connection management
- `ExternalConnectionServiceBase` - Base implementation for connection services
- Command interfaces for connection discovery and management

#### FractalDataWorks.Services.ExternalConnections.MsSql
SQL Server external connection implementation:
- `MsSqlExternalConnection` - SQL Server connection implementation
- `MsSqlConfiguration` - SQL Server configuration
- `MsSqlConnectionFactory` - Factory for SQL Server connections
- `MsSqlCommandTranslator` - Translates universal commands to SQL
#### FractalDataWorks.DependencyInjection
DI container abstractions:
- Container-agnostic dependency injection patterns
- Service registration extensions

#### FractalDataWorks.Tools
Common utilities and helpers:
- Extension methods and utility classes
- Tool type factories and base classes

#### FractalDataWorks.Hosts  
Web and worker host abstractions:
- Host service abstractions
- Background service patterns

#### FractalDataWorks.Data
Entity base classes and data patterns:
- `EntityBase` - Base class for entities with audit fields
- `GuidEntityBase` - Entity base with GUID primary keys
- `DataOperation` - Enumeration for data operations
- Query parser interfaces

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

### Core Foundation (Layer 0.5)
- [FractalDataWorks.Configuration.Abstractions](src/FractalDataWorks.Configuration.Abstractions/) - Configuration abstractions
- [FractalDataWorks.Services.Abstractions](src/FractalDataWorks.Services.Abstractions/) - Service abstractions
- [FractalDataWorks.Results](src/FractalDataWorks.Results/) - Result pattern implementations
- [FractalDataWorks.Messages](src/FractalDataWorks.Messages/) - Message abstractions

### Layer 1 Packages
- [FractalDataWorks.Services](src/FractalDataWorks.Services/README.md) - Service patterns and base implementations
- [FractalDataWorks.Configuration](src/FractalDataWorks.Configuration/README.md) - Configuration management system
- [FractalDataWorks.Services.DataProvider.Abstractions](src/FractalDataWorks.Services.DataProvider.Abstractions/README.md) - Data provider abstractions
- [FractalDataWorks.Services.DataProvider](src/FractalDataWorks.Services.DataProvider/) - Data provider implementations
- [FractalDataWorks.Services.ExternalConnections.Abstractions](src/FractalDataWorks.Services.ExternalConnections.Abstractions/README.md) - External connection abstractions
- [FractalDataWorks.Services.ExternalConnections.MsSql](src/FractalDataWorks.Services.ExternalConnections.MsSql/) - SQL Server external connections
- [FractalDataWorks.Data](src/FractalDataWorks.Data/README.md) - Data access abstractions and entity base classes
- [FractalDataWorks.DependencyInjection](src/FractalDataWorks.DependencyInjection/README.md) - DI container abstractions
- [FractalDataWorks.Hosts](src/FractalDataWorks.Hosts/README.md) - Host service abstractions
- [FractalDataWorks.Tools](src/FractalDataWorks.Tools/README.md) - Common utilities and helpers

### Additional Services
- [FractalDataWorks.Services.Scheduling.Abstractions](src/FractalDataWorks.Services.Scheduling.Abstractions/README.md) - Scheduling service abstractions
- [FractalDataWorks.Services.SecretManagement.Abstractions](src/FractalDataWorks.Services.SecretManagement.Abstractions/README.md) - Secret management abstractions
- [FractalDataWorks.Services.Transformations.Abstractions](src/FractalDataWorks.Services.Transformations.Abstractions/README.md) - Data transformation abstractions

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
public class MyService : ServiceBase<MyCommand, MyConfiguration, MyService>
{
    public MyService(ILogger<MyService> logger, MyConfiguration configuration)
        : base(logger, configuration)
    {
    }

    protected override async Task<IFdwResult<TResult>> ExecuteCore<TResult>(MyCommand command)
    {
        // Implementation with automatic validation and error handling
    }
}
```

### Configuration Management
```csharp
// Recommended approach using FdwConfigurationBase with FluentValidation.Results.ValidationResult
public class MyConfiguration : FdwConfigurationBase<MyConfiguration>
{
    public string ConnectionString { get; set; } = string.Empty;
    public int Timeout { get; set; } = 30;
    
    protected override IValidator<MyConfiguration>? GetValidator()
    {
        return new MyConfigurationValidator();
    }
}

internal sealed class MyConfigurationValidator : AbstractValidator<MyConfiguration>
{
    public MyConfigurationValidator()
    {
        RuleFor(x => x.ConnectionString)
            .NotEmpty()
            .WithMessage("Connection string is required.");
            
        RuleFor(x => x.Timeout)
            .GreaterThan(0)
            .WithMessage("Timeout must be positive.");
    }
}
```

### Configuration Validation

Configuration classes use FluentValidation with `FluentValidation.Results.ValidationResult` for consistent validation:

#### Built-in Validation with FdwConfigurationBase
The `FdwConfigurationBase<T>` class provides built-in validation support using FluentValidation:

```csharp
public class MyConfiguration : FdwConfigurationBase<MyConfiguration>
{
    public string ConnectionString { get; set; } = string.Empty;
    public int Timeout { get; set; } = 30;
    
    protected override IValidator<MyConfiguration>? GetValidator()
    {
        return new MyConfigurationValidator();
    }
}

internal sealed class MyConfigurationValidator : AbstractValidator<MyConfiguration>
{
    public MyConfigurationValidator()
    {
        RuleFor(x => x.ConnectionString)
            .NotEmpty()
            .WithMessage("Connection string is required.");
            
        RuleFor(x => x.Timeout)
            .GreaterThan(0)
            .WithMessage("Timeout must be positive.");
    }
}

// Usage - validation is built into the base class
var config = new MyConfiguration();
FluentValidation.Results.ValidationResult result = config.Validate();
if (result.IsValid)
{
    // Configuration is valid
}
else
{
    // Handle validation errors
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"Property: {error.PropertyName}, Error: {error.ErrorMessage}");
    }
}
```

#### Legacy ConfigurationBase (Deprecated)
The older `ConfigurationBase<T>` returned `IFdwResult` but is now deprecated in favor of `FdwConfigurationBase<T>` which uses the standard FluentValidation result type for better interoperability.

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
public class DataProviderService : IDataProvider
{
    private readonly IExternalDataConnectionProvider _connectionProvider;
    private readonly ILogger<DataProviderService> _logger;
    
    public DataProviderService(
        IExternalDataConnectionProvider connectionProvider,
        ILogger<DataProviderService> logger)
    {
        _connectionProvider = connectionProvider;
        _logger = logger;
    }
    
    public async Task<IFdwResult<TResult>> Execute<TResult>(DataCommandBase command)
    {
        // Provider selects appropriate connection based on command configuration
        var connection = await _connectionProvider.GetConnection(command.DataStoreConfiguration);
        return await connection.Execute<TResult>(command);
    }
}

// SQL Server external connection implementation
public class MsSqlExternalConnection : IExternalConnection
{
    private readonly MsSqlConfiguration _configuration;
    private readonly MsSqlCommandTranslator _translator;
    
    public MsSqlExternalConnection(
        MsSqlConfiguration configuration,
        MsSqlCommandTranslator translator)
    {
        _configuration = configuration;
        _translator = translator;
    }
    
    public async Task<IFdwResult<TResult>> Execute<TResult>(DataCommandBase command)
    {
        // Translate universal command to SQL Server specific implementation
        var sqlCommand = _translator.Translate(command);
        // Execute against SQL Server
        return await ExecuteSqlCommand<TResult>(sqlCommand);
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

✅ **IMPLEMENTED** - Enhanced Enum Type Factories provide complete compile-time safe factory registration

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

### External Connection Service Types

The framework includes specialized Enhanced Enum types for external connection services that enable the data gateway pattern:

```csharp
[EnumOption]
public sealed class MsSqlConnectionType : ExternalConnectionServiceTypeBase<MsSqlExternalConnectionService, MsSqlConfiguration>
{
    public MsSqlConnectionType() : base(1, "MsSql", "Microsoft SQL Server external connection service")
    {
    }
    
    public override string[] SupportedDataStores => new[] { "SqlServer", "MSSQL", "Microsoft SQL Server" };
    public override string ProviderName => "Microsoft.Data.SqlClient";
    public override IReadOnlyList<string> SupportedConnectionModes => new[] { "ReadWrite", "ReadOnly", "Bulk", "Streaming" };
    public override int Priority => 100;

    public override IServiceFactory<MsSqlExternalConnectionService, MsSqlConfiguration> CreateTypedFactory()
    {
        return new MsSqlConnectionFactory();
    }
}

// The DataProvider uses these types to route commands to appropriate connections
var supportedTypes = connectionTypes
    .Where(ct => ct.SupportedDataStores.Contains(dataStoreConfig.DataStoreType))
    .OrderByDescending(ct => ct.Priority)
    .First();
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