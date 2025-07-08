# FractalDataWorks SDK Architecture Guide

## Table of Contents
1. Introduction
2. Core Philosophy
3. Architecture Overview
4. Project Hierarchy
5. Data Access Architecture
6. Design Decisions
7. **Async Patterns and Result Types** *(New)*
8. Extension Patterns
9. Creating New Standards

## Introduction

The FractalDataWorks SDK is a .NET framework designed to provide automatic service discovery, registration, and validation through convention-based patterns. It includes a universal data access layer that allows querying any data source (SQL Server, REST APIs, GraphQL, flat files, CosmosDB) using a common LINQ interface. This guide explains the architectural decisions and provides guidance for extending the framework.

## Core Philosophy

### Framework, Not a Project
This is an SDK that other projects consume. The framework handles:
- Automatic service discovery and registration
- Cross-cutting concerns (logging, validation, error handling)
- Configuration management with strongly-typed objects
- Dependency injection setup
- Universal data access through LINQ translation

### Conventions Enable Configuration
The framework makes everything configurable by following strict conventions and contracts. Because all services follow the same patterns, the framework can provide rich configuration options that work consistently across all features. Standard conventions enable automatic discovery, validation, and registration.

### Everything Is Configurable
- Every service has a strongly-typed configuration object
- Configuration objects self-validate through `IsValid` property
- Provider-specific settings stored in flexible `Datum` dictionaries
- Configuration drives behavior at runtime
- Changes require only configuration updates, not code changes

### Fail-Safe Design
The framework prioritizes stability over exceptions:
- Methods return `bool` with `out` parameters for synchronous operations
- Async methods return `Result<T>` types (discriminated union pattern)
- Invalid states use Null Object pattern (`Invalid{Type}`)
- Validation happens automatically in base classes

### Universal Data Access
Write data access code once, run it against any provider:
- Common LINQ interface for all queries
- Provider-specific translation handled by adapters
- Switch between SQL Server, REST, GraphQL, flat files through configuration

## Architecture Overview

### Dependency Flow Principles

```
┌─────────────────┐
│   .Net (Core)   │ ← No dependencies
└────────┬────────┘
         │
┌────────▼────────┐
│   .Services     │ ← Depends only on Core
└────────┬────────┘
         │
    ┌────┴────┬────────────┐
    │         │            │
┌───▼───┐ ┌──▼──────┐ ┌───▼───────┐
│Services│ │Connections│ │Connections│
│.{Feature}│ │.Data     │ │.{Provider}│
└────────┘ └──────────┘ └───────────┘
```

**Key Rules:**
- Dependencies flow inward/downward only
- No circular dependencies
- No horizontal dependencies between features
- Data access uses universal interface pattern

### Core Patterns

1. **Service Pattern**: All services inherit from `ServiceBase<TConfiguration>`
2. **Configuration Pattern**: Strongly-typed, self-validating configurations
3. **Factory Pattern**: Safe object creation with validation
4. **Data Access Pattern**: Universal LINQ interface with provider adapters
5. **Auto-Discovery**: Enum-based service registration
6. **Builder Pattern**: Complex object construction with validation
7. **Result Pattern**: Discriminated union-like types for async operations

### Why This Architecture?

1. **Testability**: Interfaces in Core can be mocked without referencing implementations
2. **Modularity**: Features can be added/removed without affecting others
3. **Discoverability**: All contracts in predictable locations
4. **Extensibility**: New features follow established patterns
5. **Configurability**: Standard patterns enable rich configuration for all services

## Project Hierarchy

### 1. Core Project (`{Company}.{Product}.Net`)

**Purpose**: Define all contracts, interfaces, and shared types

**Contains:**
- Core interfaces: `IEntity`, `IValueObject`, `ISpecification`
- Service interfaces: `IGenericService`, `IAsyncService<TRequest, TResponse>`
- Configuration interfaces: `IEnhancedConfiguration`, `IEnhancedConfiguration<T>`
- Result types: `Result<T>`, `Unit`, `ServiceMessage`
- No implementations, no abstract classes (except for Result<T> pattern)

**Why**: Single source of truth for all contracts. Zero dependencies ensures it never causes version conflicts.

### 2. Services Project (`{Company}.{Product}.Services`)

**Purpose**: Provide base implementations and service-specific interfaces

**Contains:**
```csharp
// Abstract base classes with generic validation
public abstract class ServiceBase<TConfiguration> 
    where TConfiguration : ConfigurationBase
{
    protected ServiceBase(ILogger logger, TConfiguration configuration)
    {
        // Validation happens here automatically
        if (!configuration.IsValid)
        {
            _configuration = (TConfiguration)InvalidConfiguration.Instance;
        }
    }
}

// Service-specific interfaces
public interface ISessionManagementService : IGenericService
{
    // Synchronous methods (legacy)
    bool CreateProject(Project project, out ServiceMessage message);
    
    // Async methods (preferred)
    Task<Result<Project>> CreateProjectAsync(CreateProjectCommand command);
}
```

**Why**: Centralizes cross-cutting concerns. Generic constraints ensure type safety. Validation in constructor prevents invalid states. Rich configuration options available to all services through inheritance.

### 3. Feature Projects (`{Company}.{Product}.Services.{Feature}`)

**Purpose**: Implement specific business logic

**Contains:**
```csharp
public class SessionManagementService : 
    ServiceBase<SessionManagementConfiguration>, 
    ISessionManagementService
{
    private readonly IMediator _mediator;
    
    // Modern async implementation
    public async Task<Result<Project>> CreateProjectAsync(CreateProjectCommand command)
    {
        return await _mediator.Send(command);
    }
    
    // Legacy sync wrapper for backward compatibility
    public bool CreateProject(Project project, out ServiceMessage message)
    {
        var command = new CreateProjectCommand(project);
        var result = CreateProjectAsync(command)
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();
            
        return result.Match(
            success: (created, msg) => 
            {
                project.Id = created.Id;
                message = ServiceMessage.Success(msg);
                return true;
            },
            failure: error =>
            {
                message = error;
                return false;
            }
        );
    }
}
```

**Why**: Developers focus only on business logic. Framework handles validation, logging, configuration, and all cross-cutting concerns.

## Design Decisions

### 1. No Layer Names in Namespaces

**Decision**: Use `FractalDataWorks.Services.SessionManagement` not `FractalDataWorks.Infrastructure.Services`

**Rationale**: 
- Namespaces should describe capabilities, not technical layers
- Reduces namespace length
- Layer information available from solution structure

### 2. Configuration as First-Class Citizen

**Decision**: Every service has strongly-typed configuration

**Rationale**:
- Compile-time safety for configuration
- Self-validating configurations
- Testable configuration logic

### 3. Bool Return with Out Parameters (Synchronous Methods)

**Decision**: `bool Execute(command, out result, out message)` for synchronous operations

**Rationale**:
- Clear success/failure semantics
- Forces handling of failure cases
- Better performance (no exception overhead)
- Consistent pattern across synchronous framework methods

### 4. Result<T> Pattern for Async Methods

**Decision**: `Task<Result<T>> ExecuteAsync(command)` for asynchronous operations

**Rationale**:
- Works naturally with async/await
- Mimics future discriminated union syntax
- Type-safe error handling
- Composable with LINQ and other functional patterns
- Easy migration path when native DUs land

### 5. Validation in Generic Base Classes

**Decision**: Validation logic in `ConfigurationBase<T>` and `ServiceBase<T>`

**Rationale**:
- DRY principle - write validation once
- Impossible to forget validation
- Consistent validation across all services

## Async Patterns and Result Types

### The Result<T> Pattern

Located in `FractalDataWorks.Net/Models/Result.cs`:

```csharp
namespace FractalDataWorks.Net.Models;

/// <summary>
/// Discriminated union-like result type for async operations.
/// Designed to be easily refactored when native discriminated unions land in C#.
/// </summary>
public abstract record Result<T>
{
    private Result() { }
    
    public sealed record Success(T Value, string Message = "Success") : Result<T>;
    public sealed record Failure(ServiceMessage Error) : Result<T>;
    
    // Factory methods
    public static Result<T> Ok(T value, string message = "Success") 
        => new Success(value, message);
        
    public static Result<T> Fail(string error) 
        => new Failure(ServiceMessage.Error(error));
        
    public static Result<T> Fail(ServiceMessage error) 
        => new Failure(error);
    
    // Pattern matching helper
    public TResult Match<TResult>(
        Func<T, string, TResult> success,
        Func<ServiceMessage, TResult> failure) =>
        this switch
        {
            Success(var value, var message) => success(value, message),
            Failure(var error) => failure(error),
            _ => throw new InvalidOperationException("Invalid result state")
        };
    
    // Convenience properties for transition
    public bool IsSuccess => this is Success;
    public bool IsFailure => this is Failure;
}
```

### Unit Type for Void Operations

Located in `FractalDataWorks.Net/Models/Unit.cs`:

```csharp
namespace FractalDataWorks.Net.Models;

/// <summary>
/// Unit type representing void/no return value in Result patterns
/// </summary>
public readonly record struct Unit
{
    public static readonly Unit Value = new();
}
```

### Result Extensions

Located in `FractalDataWorks.Net/Models/ResultExtensions.cs`:

```csharp
namespace FractalDataWorks.Net.Models;

public static class ResultExtensions
{
    // Convenience method for void operations
    public static Result<Unit> Ok(string message = "Success") 
        => Result<Unit>.Ok(Unit.Value, message);
        
    // Async mapping operations
    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<TOut>> mapper)
    {
        var result = await resultTask;
        return result switch
        {
            Result<TIn>.Success(var value, var msg) => 
                Result<TOut>.Ok(await mapper(value), msg),
            Result<TIn>.Failure(var error) => 
                Result<TOut>.Fail(error),
            _ => throw new InvalidOperationException()
        };
    }
}
```

### Usage Examples

#### Service Implementation
```csharp
public async Task<Result<Project>> CreateProjectAsync(CreateProjectCommand command)
{
    // Validation
    if (string.IsNullOrWhiteSpace(command.Name))
        return Result<Project>.Fail("Project name is required");
    
    try
    {
        var project = await _mediator.Send(command);
        return Result<Project>.Ok(project, "Project created successfully");
    }
    catch (DuplicateNameException)
    {
        return Result<Project>.Fail("A project with this name already exists");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating project");
        return Result<Project>.Fail($"An error occurred: {ex.Message}");
    }
}
```

#### Pattern Matching
```csharp
var result = await service.CreateProjectAsync(command);

// Switch expression
var response = result switch
{
    Result<Project>.Success(var project, var message) => 
        Ok(new { project.Id, message }),
    Result<Project>.Failure(var error) => 
        BadRequest(new { error }),
    _ => StatusCode(500)
};

// Match method
var summary = result.Match(
    success: (project, msg) => $"Created: {project.Name}",
    failure: error => $"Failed: {error.Message}"
);
```

#### Void Operations
```csharp
public async Task<Result<Unit>> DeleteProjectAsync(string projectId)
{
    if (string.IsNullOrWhiteSpace(projectId))
        return Result<Unit>.Fail("Project ID is required");
        
    var deleted = await _repository.DeleteAsync(projectId);
    
    return deleted 
        ? ResultExtensions.Ok("Project deleted successfully")
        : Result<Unit>.Fail("Project not found");
}
```

### Bridge Pattern for Sync/Async

For services that need both patterns during transition:

```csharp
public class ProjectService : ServiceBase<ProjectConfiguration>, IProjectService
{
    // Modern async method (preferred)
    public async Task<Result<Project>> CreateProjectAsync(CreateProjectCommand command)
    {
        return await _mediator.Send(command);
    }
    
    // Legacy sync method (for backward compatibility)
    public bool CreateProject(Project project, out ServiceMessage message)
    {
        var command = new CreateProjectCommand(project);
        
        // Bridge to async - use carefully to avoid deadlocks
        var result = Task.Run(async () => await CreateProjectAsync(command))
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();
        
        return result.Match(
            success: (created, msg) =>
            {
                // Copy properties back to input object
                project.Id = created.Id;
                project.CreatedAt = created.CreatedAt;
                message = ServiceMessage.Success(msg);
                return true;
            },
            failure: error =>
            {
                message = error;
                return false;
            }
        );
    }
}
```

### MediatR Integration

Commands and queries should return Result<T>:

```csharp
// Command
public record CreateProjectCommand : IRequest<Result<Project>>
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}

// Handler
public class CreateProjectCommandHandler : 
    IRequestHandler<CreateProjectCommand, Result<Project>>
{
    public async Task<Result<Project>> Handle(
        CreateProjectCommand request, 
        CancellationToken cancellationToken)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result<Project>.Fail("Project name is required");
        
        // Check for duplicates
        var existing = await _repository
            .FindByNameAsync(request.Name, cancellationToken);
            
        if (existing != null)
            return Result<Project>.Fail($"Project '{request.Name}' already exists");
        
        // Create project
        var project = new Project
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };
        
        await _repository.AddAsync(project, cancellationToken);
        
        return Result<Project>.Ok(project, $"Project '{project.Name}' created");
    }
}
```

### Migration Strategy

1. **New Services**: Use `Result<T>` for all async methods
2. **Existing Services**: Add async overloads that return `Result<T>`
3. **Gradual Migration**: Keep bool/out methods during transition
4. **Future Refactoring**: When C# adds native DUs, minimal changes needed

### Best Practices

1. **Prefer async methods** with `Result<T>` for new development
2. **Use pattern matching** instead of IsSuccess checks
3. **Keep error messages user-friendly** in Result failures
4. **Log exceptions** before returning Result.Fail
5. **Use Unit type** for void operations, not `Result<object>`
6. **Avoid mixing** async and sync unnecessarily

## Extension Patterns

### Adding a New Service Type

1. **Define Interface in Core**:
```csharp
// In .Net project
public interface ITranslationService : IGenericService
{
    // Async method (preferred)
    Task<Result<TranslationResult>> TranslateAsync(
        TranslationRequest request, 
        CancellationToken cancellationToken = default);
    
    // Sync method (if needed for compatibility)    
    bool Translate(string text, string targetLanguage, 
                  out string result, out ServiceMessage message);
}
```

2. **Create Base Infrastructure in Services**:
```csharp
// In .Services project
public interface ILanguageProvider
{
    string LanguageCode { get; }
    bool CanTranslate(string fromLanguage);
}

public abstract class TranslationServiceBase<TConfig> : 
    ServiceBase<TConfig> 
    where TConfig : ConfigurationBase
{
    // Common translation logic
}
```

3. **Define Enum Base for Auto-Discovery**:
```csharp
// In source generator project
public abstract class LanguageEnumBase : Enum
{
    public abstract string LanguageCode { get; }
}
```

4. **Implement Concrete Service**:
```csharp
// In .Services.Translation project
public enum KoreanLanguageOption : LanguageEnumBase 
{
    public override string LanguageCode => "ko";
}

public class TranslationService : 
    TranslationServiceBase<TranslationConfiguration>,
    ITranslationService
{
    private readonly IMediator _mediator;
    
    public async Task<Result<TranslationResult>> TranslateAsync(
        TranslationRequest request,
        CancellationToken cancellationToken = default)
    {
        return await _mediator.Send(
            new TranslateCommand(request), 
            cancellationToken);
    }
}
```

## Summary

This architecture prioritizes:
- **Configurability**: Everything configurable through strongly-typed objects
- **Developer Experience**: Minimal boilerplate, automatic discovery
- **Maintainability**: Clear boundaries, single responsibility
- **Extensibility**: Easy to add new features and data providers
- **Reliability**: Fail-safe patterns, automatic validation
- **Flexibility**: Universal data access works with any provider
- **Modern Patterns**: Async-first with Result<T> types
- **Future-Proof**: Ready for discriminated unions

The framework handles the plumbing so developers can focus on business value. Standard conventions and contracts enable rich configuration options while maintaining consistency across all features.
