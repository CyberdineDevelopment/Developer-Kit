# FractalDataWorks.Services.DataProviders.Abstractions

**Minimal data service abstractions for the FractalDataWorks Framework.**

## Purpose

This package provides the foundational interfaces for data services in the FractalDataWorks ecosystem. It defines the domain boundary for data operations through minimal, clean abstractions.

## Architecture

The data provider abstractions follow the framework's **minimal interface pattern**:

- **Core Interface**: `IDataService` extends `IFdwService<IDataCommand>`
- **Command Contract**: `IDataCommand` defines the data command structure
- **Configuration Base**: `IDataProvidersConfiguration` provides configuration contract
- **Base Classes**: Add type constraints without implementation logic

## Key Interfaces

### Core Service Interface
```csharp
public interface IDataService : IFdwService<IDataCommand>
{
    // Inherits all service functionality from IFdwService<T>
    // No additional methods - follows minimal interface design
}
```

### Command Interface
```csharp
public interface IDataCommand
{
    // Defines data command structure
    // Implemented by concrete command types (queries, updates, etc.)
}
```

### Configuration Interface  
```csharp
public interface IDataProvidersConfiguration
{
    // Provides data provider configuration contract
    // Implemented by service-specific configurations
}
```

## Base Classes

The package includes base classes that **only add generic type constraints**:

- `DataProvidersServiceBase` - Provides typed service constraints
- `DataProvidersServiceFactoryBase` - Adds factory pattern constraints  
- `DataProvidersServiceProviderBase` - Enables service provider pattern
- `DataProvidersServiceTypeBase` - Implements service type enumeration

**Important**: Base classes contain **no implementation logic**. They exist solely to add type safety and generic constraints.

## Core Types

### Transaction Support
```csharp
public enum FdwTransactionState
{
    Active, Committed, RolledBack, InDoubt
}

public interface IDataTransaction
{
    // Provides transaction management contract
    // State tracking and lifecycle management
}
```

### Batch Processing
```csharp
public interface IBatchResult
{
    // Defines batch operation result contract
    // Success/failure tracking for multiple commands
}

public interface ICommandBuilder
{
    // Enables command construction and transformation
    // Provider-specific command building patterns
}
```

### Performance Monitoring
```csharp
public interface IProviderMetrics
{
    // Exposes data provider performance metrics
    // Execution time, throughput, error rates
}
```

## Usage

Concrete implementations should:

1. **Implement the interfaces** with actual data access logic
2. **Inherit from base classes** for type safety (optional)
3. **Define specific command types** for their data operations
4. **Provide configuration classes** for connection and behavior settings

## Generic Constraints

The type hierarchy flows from most general to most specific:

```
IFdwService<T>
    ↓
IDataService : IFdwService<IDataCommand>
    ↓  
DataProvidersServiceBase<TConfig, TCommand>
    ↓
ConcreteDataService<SpecificConfig, SpecificCommand>
```

## Framework Integration

This abstraction integrates with other FractalDataWorks services:

- **ExternalConnections**: Manages database and API connections
- **Authentication**: Handles service authentication
- **SecretManagement**: Stores connection strings and credentials
- **Transformations**: Processes data between different formats

## Design Philosophy

These abstractions are **intentionally minimal**:

- ✅ Define domain boundaries through interfaces
- ✅ Provide type safety through base classes
- ✅ Enable service discovery and dependency injection
- ❌ No implementation logic in abstractions
- ❌ No complex inheritance hierarchies
- ❌ No framework-specific coupling

## Evolution

This package will grow organically as the framework evolves:

- New interfaces added when patterns emerge
- Base classes extended with new constraints as needed
- Backward compatibility maintained for existing implementations

The minimal design ensures maximum flexibility for concrete implementations while providing a consistent service pattern across the FractalDataWorks ecosystem.