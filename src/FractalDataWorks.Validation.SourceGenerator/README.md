# FractalDataWorks.Validation.SourceGenerator

A source generator that automatically creates `IFdwResult Validate()` extension methods for configuration classes that have corresponding `AbstractValidator<T>` implementations.

## Overview

This source generator scans your compilation for classes that inherit from `AbstractValidator<T>` and automatically generates extension methods on the generic type parameter `T`. This provides a consistent validation API across all configuration classes while maintaining type safety.

## Generated Extension Methods

For each `AbstractValidator<TConfig>` found, the generator creates:

1. **Synchronous Validation**:
   ```csharp
   public static IFdwResult Validate(this TConfig config)
   ```

2. **Asynchronous Validation**:
   ```csharp
   public static async Task<IFdwResult> ValidateAsync(this TConfig config, CancellationToken cancellationToken = default)
   ```

## Example Usage

### Input: Configuration Class and Validator

```csharp
namespace MyApp.Configuration;

public sealed class DatabaseConfiguration
{
    public string ConnectionString { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
}

public sealed class DatabaseConfigurationValidator : AbstractValidator<DatabaseConfiguration>
{
    public DatabaseConfigurationValidator()
    {
        RuleFor(x => x.ConnectionString)
            .NotEmpty()
            .WithMessage("Connection string is required");
            
        RuleFor(x => x.TimeoutSeconds)
            .GreaterThan(0)
            .WithMessage("Timeout must be positive");
    }
}
```

### Generated Output

```csharp
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FractalDataWorks.Results;

namespace MyApp.Configuration;

public static partial class ValidationExtensions
{
    public static IFdwResult Validate(this DatabaseConfiguration config)
    {
        var validator = new DatabaseConfigurationValidator();
        var result = validator.Validate(config);
        
        if (result.IsValid)
            return FdwResult.Success();
        
        var errors = string.Join("; ", result.Errors.Select(e => e.ErrorMessage));
        return FdwResult.Failure($"Validation failed: {errors}");
    }
    
    public static async Task<IFdwResult> ValidateAsync(this DatabaseConfiguration config, CancellationToken cancellationToken = default)
    {
        var validator = new DatabaseConfigurationValidator();
        var result = await validator.ValidateAsync(config, cancellationToken);
        
        if (result.IsValid)
            return FdwResult.Success();
        
        var errors = string.Join("; ", result.Errors.Select(e => e.ErrorMessage));
        return FdwResult.Failure($"Validation failed: {errors}");
    }
}
```

### Usage in Application Code

```csharp
var config = new DatabaseConfiguration
{
    ConnectionString = "Server=localhost;Database=MyApp;",
    TimeoutSeconds = 30
};

// Synchronous validation
var result = config.Validate();
if (!result.IsSuccess)
{
    Console.WriteLine($"Validation failed: {result.ErrorMessage}");
}

// Asynchronous validation
var asyncResult = await config.ValidateAsync();
if (!asyncResult.IsSuccess)
{
    Console.WriteLine($"Async validation failed: {asyncResult.ErrorMessage}");
}
```

## Features

- **Incremental Source Generator**: Optimized for performance in large codebases
- **Namespace Grouping**: Extension methods are generated in the same namespace as the validators
- **Partial Classes**: Generated classes are partial, allowing for custom extensions
- **Error Aggregation**: Multiple validation errors are combined into a single error message
- **Cancellation Support**: Async methods support cancellation tokens
- **Type Safety**: Extension methods are strongly typed to the configuration class

## Requirements

- .NET Standard 2.0 or higher
- FluentValidation package
- FractalDataWorks.Results package (for IFdwResult and FdwResult)

## Installation

This source generator is automatically included when you reference the `FractalDataWorks.Validation.SourceGenerator` package. No additional setup is required.

## Integration with FractalDataWorks

This generator integrates seamlessly with the FractalDataWorks ecosystem:

- Uses `IFdwResult` for consistent result handling
- Follows FractalDataWorks naming conventions
- Generates code compatible with the framework's validation patterns
- Supports both sync and async validation workflows

## Technical Details

- **Target Framework**: netstandard2.0 for maximum compatibility
- **Code Generation**: Uses a custom code builder implementation following FractalDataWorks.CodeBuilder patterns
- **Discovery**: Automatically discovers `AbstractValidator<T>` classes at compile time
- **Output**: Generates one file per namespace containing validators
- **Performance**: Incremental generator ensures fast compilation times