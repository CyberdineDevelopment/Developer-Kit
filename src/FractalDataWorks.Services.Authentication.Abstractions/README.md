# FractalDataWorks.Services.Authentication.Abstractions

**Minimal authentication service abstractions for the FractalDataWorks Framework.**

## Purpose

This package provides the foundational interfaces for authentication services in the FractalDataWorks ecosystem. It defines the domain boundary for authentication operations through minimal, clean abstractions.

## Architecture

The authentication abstractions follow the framework's **minimal interface pattern**:

- **Core Interface**: `IAuthenticationService` extends `IFdwService<IAuthenticationCommand>`
- **Command Contract**: `IAuthenticationCommand` defines the command structure
- **Configuration Base**: `IAuthenticationConfiguration` provides configuration contract
- **Base Classes**: Add type constraints without implementation logic

## Key Interfaces

### Core Service Interface
```csharp
public interface IAuthenticationService : IFdwService<IAuthenticationCommand>
{
    // Inherits all service functionality from IFdwService<T>
    // No additional methods - follows minimal interface design
}
```

### Command Interface
```csharp
public interface IAuthenticationCommand
{
    // Defines authentication command structure
    // Implemented by concrete command types
}
```

### Configuration Interface
```csharp
public interface IAuthenticationConfiguration
{
    // Provides authentication configuration contract
    // Implemented by service-specific configurations
}
```

## Base Classes

The package includes base classes that **only add generic type constraints**:

- `AuthenticationServiceBase` - Provides typed service constraints
- `AuthenticationServiceFactoryBase` - Adds factory pattern constraints  
- `AuthenticationServiceProviderBase` - Enables service provider pattern
- `AuthenticationServiceTypeBase` - Implements service type enumeration

**Important**: Base classes contain **no implementation logic**. They exist solely to add type safety and generic constraints.

## Usage

Concrete implementations should:

1. **Implement the interfaces** with actual authentication logic
2. **Inherit from base classes** for type safety (optional)
3. **Define specific command types** for their authentication methods
4. **Provide configuration classes** for their specific requirements

## Generic Constraints

The type hierarchy flows from most general to most specific:

```
IFdwService<T>
    ↓
IAuthenticationService : IFdwService<IAuthenticationCommand>
    ↓  
AuthenticationServiceBase<TCommand, TConfiguration, TService>
    ↓
ConcreteAuthenticationService<SpecificCommand, SpecificConfiguration, ConcreteAuthenticationService>
```

## Framework Integration

This abstraction integrates with other FractalDataWorks services:

- **SecretManagement**: Stores API keys and credentials
- **ExternalConnections**: Connects to identity providers
- **DataProviders**: Persists authentication state

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