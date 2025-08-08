# FractalDataWorks.Services.SecretManagement.Abstractions

**Minimal secret management service abstractions for the FractalDataWorks Framework.**

## Purpose

This package provides the foundational interfaces for secret management services in the FractalDataWorks ecosystem. It defines the domain boundary for secure secret operations through minimal, clean abstractions.

## Architecture

The secret management abstractions follow the framework's **minimal interface pattern**:

- **Core Interface**: `ISecretService` extends `IFdwService<ISecretCommand>`
- **Command Contract**: `ISecretCommand` defines the secret command structure
- **Configuration Base**: `ISecretManagementConfiguration` provides configuration contract
- **Base Classes**: Add type constraints without implementation logic

## Key Interfaces

### Core Service Interface
```csharp
public interface ISecretService : IFdwService<ISecretCommand>
{
    // Inherits all service functionality from IFdwService<T>
    // No additional methods - follows minimal interface design
}
```

### Command Interface
```csharp
public interface ISecretCommand
{
    // Defines secret command structure
    // Implemented by concrete command types (get, set, delete, etc.)
}
```

### Configuration Interface
```csharp
public interface ISecretManagementConfiguration
{
    // Provides secret management configuration contract
    // Implemented by service-specific configurations
}
```

## Base Classes

The package includes base classes that **only add generic type constraints**:

- `SecretManagementServiceBase` - Provides typed service constraints
- `SecretProviderBase` - Adds secret provider pattern constraints

**Important**: Base classes contain **no implementation logic**. They exist solely to add type safety and generic constraints.

## Core Types

### Secret Management
```csharp
public interface ISecretManager
{
    // High-level secret management operations
    // Orchestrates multiple secret services
}

public interface ISecretContainer
{
    // Logical grouping of secrets (vault, key ring, etc.)
    // Provides container-level operations
}
```

### Secret Metadata
```csharp
public interface ISecretMetadata
{
    // Secret information without exposing the actual value
    // Created date, expiry, tags, permissions
}

public class SecretValue
{
    // Represents a secret value with metadata
    // Secure handling and disposal patterns
}
```

### Security Features
```csharp
public enum HealthStatus
{
    Healthy, Degraded, Unhealthy, Unknown
}

public interface ISecretServiceHealth
{
    // Health monitoring for secret services
    // Availability and performance tracking
}
```

### Batch Operations
```csharp
public interface ISecretBatchResult
{
    // Results from batch secret operations
    // Success/failure tracking for multiple commands
}

public interface ISecretOperationMetrics
{
    // Performance metrics for secret operations
    // Latency, throughput, error rates
}
```

## Usage

Concrete implementations should:

1. **Implement the interfaces** with actual secret management logic
2. **Inherit from base classes** for type safety (optional)
3. **Define specific command types** for their secret operations
4. **Provide configuration classes** for service-specific settings
5. **Handle security properly** with encryption and secure disposal

## Generic Constraints

The type hierarchy flows from most general to most specific:

```
IFdwService<T>
    ↓
ISecretService : IFdwService<ISecretCommand>
    ↓  
SecretManagementServiceBase<TConfig, TCommand>
    ↓
ConcreteSecretService<SpecificConfig, SpecificCommand>
```

## Security Considerations

This abstraction layer handles sensitive data:

- **No secret values in interfaces** - only metadata and operations
- **Secure disposal patterns** - IDisposable implementations required
- **Encryption support** - built into value types
- **Health monitoring** - detect compromised services
- **Audit logging** - track access and modifications

## Framework Integration

This abstraction integrates with other FractalDataWorks services:

- **Authentication**: Uses secrets for API keys and certificates
- **ExternalConnections**: Stores connection strings and credentials
- **DataProviders**: Secures database passwords and keys
- **Configuration**: Provides secure configuration values

## Design Philosophy

These abstractions are **intentionally minimal**:

- ✅ Define domain boundaries through interfaces
- ✅ Provide type safety through base classes
- ✅ Enable service discovery and dependency injection
- ✅ Prioritize security over convenience
- ❌ No implementation logic in abstractions
- ❌ No complex inheritance hierarchies
- ❌ No secret values exposed in interfaces

## Evolution

This package will grow organically as the framework evolves:

- New interfaces added when patterns emerge
- Base classes extended with new constraints as needed
- Security features enhanced based on best practices
- Backward compatibility maintained for existing implementations

The minimal design ensures maximum flexibility for concrete implementations while providing a consistent, secure service pattern across the FractalDataWorks ecosystem.