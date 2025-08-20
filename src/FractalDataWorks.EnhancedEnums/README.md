# FractalDataWorks.EnhancedEnums

Core library for Enhanced Enums providing base classes, attributes, and builders for type-safe, extensible enum implementations in C#.

## Table of Contents

- [Overview](#overview)
- [Installation](#installation)
- [Basic Usage](#basic-usage)
- [Advanced Features](#advanced-features)
- [Generated Code](#generated-code)
- [Performance Characteristics](#performance-characteristics)
- [Best Practices](#best-practices)
- [Attribute Reference](#attribute-reference)
- [How It Works](#how-it-works)

## Overview

FractalDataWorks.EnhancedEnums is a source generator that creates type-safe, object-oriented alternatives to traditional C# enums. It generates static collection classes with efficient lookup methods for enum-like types.

### Key Benefits

- **Type Safety**: Compile-time validation of enum definitions
- **Rich Behavior**: Object-oriented enum instances with methods and properties
- **Efficient Lookups**: Generated lookup methods for fast value retrieval
- **Extensibility**: Easy to add new enum values without breaking existing code
- **Source Generation**: Zero runtime overhead, everything is generated at compile time

## Installation

```bash
# For source generation scenarios (recommended)
dotnet add package FractalDataWorks.EnhancedEnums.SourceGenerators

# For manual builder usage only
dotnet add package FractalDataWorks.EnhancedEnums
```

## Debugging Generated Files

To see the generated source files on disk for debugging purposes, add this to your project file:

```xml
<PropertyGroup>
  <EmitGeneratorFiles>true</EmitGeneratorFiles>
</PropertyGroup>
```

This will automatically:
- Set `EmitCompilerGeneratedFiles` to true
- Set `CompilerGeneratedFilesOutputPath` to "GeneratedFiles" (unless already specified)
- Exclude the generated files from compilation to prevent double compilation

The generated files will appear in the `GeneratedFiles` folder in your project directory.

## Basic Usage

### 1. Define an Enhanced Enum (Collection-First Pattern)

```csharp
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

// Collection-first pattern: Define collection class with generic constraint
[EnumCollection(CollectionName = "Priorities")]
public abstract class PriorityCollection<T> where T : Priority
{
    // The generic constraint T indicates what type to collect
}

// Base class is separate from the collection definition
public abstract class Priority : EnumOptionBase<Priority>
{
    public abstract int Level { get; }
    public abstract string Description { get; }
    
    protected Priority(int id, string name, int level, string description) : base(id, name)
    {
        Level = level;
        Description = description;
    }
}

[EnumOption]
public class High : Priority
{
    public High() : base(1, "High", 1, "High priority item") { }
}

[EnumOption]
public class Medium : Priority
{
    public Medium() : base(2, "Medium", 2, "Medium priority item") { }
}

[EnumOption]
public class Low : Priority
{
    public Low() : base(3, "Low", 3, "Low priority item") { }
}
```

### 2. Use the Generated Collection

```csharp
// Access all values
foreach (var priority in Priorities.All())
{
    Console.WriteLine($"{priority.Name}: {priority.Description}");
}

// Lookup by name
var high = Priorities.GetByName("High");
Console.WriteLine($"Level: {high?.Level}");

// Factory method (creates new instance)
var mediumFactory = Priorities.Medium();

// Check if value exists
var unknown = Priorities.GetByName("Unknown");
if (unknown == null)
{
    Console.WriteLine("Priority not found");
}
```

## Advanced Features

### Lookup Properties

Mark properties with `[EnumLookup]` to generate dedicated lookup methods:

```csharp
// Collection class with attributes
[EnumCollection(CollectionName = "HttpStatusCodes")]
public abstract class HttpStatusCodeCollection<T> where T : HttpStatusCode
{
}

// Base class with lookup properties
public abstract class HttpStatusCode : EnumOptionBase<HttpStatusCode>
{
    [EnumLookup("GetByCode")]
    public int Code { get; }
    
    [EnumLookup("GetByCategory")]
    public string Category { get; }
    
    protected HttpStatusCode(int id, string name, int code, string category) : base(id, name)
    {
        Code = code;
        Category = category;
    }
}

[EnumOption]
public class Ok : HttpStatusCode
{
    public Ok() : base(1, "OK", 200, "Success") { }
}

[EnumOption]
public class NotFound : HttpStatusCode
{
    public NotFound() : base(2, "Not Found", 404, "Client Error") { }
}

// Usage:
var ok = HttpStatusCodes.GetByCode(200);
var clientError = HttpStatusCodes.GetByCategory("Client Error");
```

### Custom Collection Names

```csharp
[EnumCollection(CollectionName = "MyCustomStatuses")]
public abstract class StatusCollection<T> where T : Status
{
}

public abstract class Status : EnumOptionBase<Status>
{
    protected Status(int id, string name) : base(id, name) { }
}

// Generates: MyCustomStatuses.All(), MyCustomStatuses.GetByName()
```

### String Comparison Options

```csharp
[EnumCollection(
    CollectionName = "CaseSensitiveEnums",
    NameComparison = StringComparison.Ordinal)]
public abstract class CaseSensitiveEnumCollection<T> where T : CaseSensitiveEnum
{
}

public abstract class CaseSensitiveEnum : EnumOptionBase<CaseSensitiveEnum>
{
    protected CaseSensitiveEnum(int id, string name) : base(id, name) { }
}
```

### Factory vs Singleton Patterns

```csharp
[EnumCollection(
    CollectionName = "DatabaseConnections",
    UseSingletonInstances = false)]  // Factory pattern: creates new instances
public abstract class DatabaseConnection : EnumOptionBase<DatabaseConnection>
{
    public string ConnectionString { get; }
    
    protected DatabaseConnection(int id, string name, string connectionString) : base(id, name)
    {
        ConnectionString = connectionString;
    }
}

// When UseSingletonInstances = false:
// - All() creates new instances each call
// - GetByName() creates new instances
// - Factory methods create new instances
```

### Custom Lookup Method Names

```csharp
[EnumCollection(CollectionName = "Users")]
public abstract class User : EnumOptionBase<User>
{
    [EnumLookup("FindByRole")]
    public string Role { get; }
    
    protected User(int id, string name, string role) : base(id, name)
    {
        Role = role;
    }
}

// Generates: FindByRole() instead of GetByRole()
```

## Generated Code

The source generator creates static collection classes with the following structure:

```csharp
public static class [CollectionName]
{
    private static readonly ImmutableArray<[EnumType]> _all;
    private static readonly FrozenDictionary<string, [EnumType]> _nameDict;
    // Additional dictionaries for custom lookups
    
    static [CollectionName]()
    {
        // Initialize all enum instances (singleton pattern)
        var items = new List<[EnumType]>
        {
            new [EnumOption1](),
            new [EnumOption2](),
            // ...
        };
        
        _all = items.ToImmutableArray();
        _nameDict = items.ToFrozenDictionary(x => x.Name, StringComparer.Ordinal);
    }
    
    /// <summary>
    /// Gets all available [EnumType] values.
    /// </summary>
    public static ImmutableArray<[EnumType]> All() => _all;
    
    /// <summary>
    /// Gets the [EnumType] with the specified name.
    /// </summary>
    public static [EnumType]? GetByName(string name) => 
        _nameDict.TryGetValue(name, out var result) ? result : null;
    
    /// <summary>
    /// Factory method for [EnumOption1] (if GenerateFactoryMethods = true).
    /// </summary>
    public static [EnumType] [EnumOption1]() => GetByName("[EnumOption1]")!;
    
    // Additional lookup methods for [EnumLookup] properties
    public static [EnumType]? GetBy[PropertyName]([PropertyType] value) => /* lookup logic */;
}
```

## Performance Characteristics

### Current Implementation

- **Initialization**: O(n) - All enum instances created during static constructor
- **All() Method Access**: O(1) - Returns cached `ImmutableArray`
- **Name Lookups**: O(1) - Uses `FrozenDictionary` (.NET 8+) or `ImmutableDictionary` for fast lookups
- **Custom Property Lookups**: O(1) - Uses dedicated dictionaries for each [EnumLookup] property
- **Memory Usage**: All instances stored in memory permanently (singleton pattern)

### Performance Benefits

- **Fast Lookups**: Dictionary-based lookups provide O(1) performance
- **Memory Efficient**: Singleton pattern ensures only one instance per enum value
- **Compile-time Safety**: All lookup methods are generated and type-safe
- **Zero Runtime Overhead**: Everything is generated at compile time

## Best Practices

### 1. Naming Conventions

```csharp
// Good: Descriptive base class name
[EnhancedEnumOption]
public abstract class OrderStatus { }

// Good: Descriptive option names
[EnumOption]
public class AwaitingPayment : OrderStatus { }
```

### 2. Keep Enums Focused

```csharp
// Good: Single responsibility
[EnhancedEnumOption]
public abstract class PaymentMethod
{
    public abstract string Name { get; }
    public abstract bool RequiresVerification { get; }
}

// Avoid: Too many responsibilities
[EnhancedEnumOption]
public abstract class EverythingEnum
{
    public abstract string PaymentMethod { get; }
    public abstract string UserRole { get; }
    public abstract string OrderStatus { get; }
}
```

### 3. Use Lookup Properties Strategically

```csharp
[EnhancedEnumOption]
public abstract class Country
{
    public abstract string Name { get; }
    
    // Good: Frequently searched properties
    [EnumLookup]
    public abstract string IsoCode { get; }
    
    // Consider: Only add lookup if needed
    public abstract string Capital { get; }
}
```

### 4. Document Your Enums

```csharp
/// <summary>
/// Represents the status of an order in the system.
/// </summary>
[EnhancedEnumOption]
public abstract class OrderStatus
{
    /// <summary>
    /// Gets the display name of the status.
    /// </summary>
    public abstract string Name { get; }
    
    /// <summary>
    /// Gets the internal status code.
    /// </summary>
    [EnumLookup]
    public abstract string Code { get; }
}
```

## Attribute Reference

### EnumCollectionAttribute

Marks a class as an enhanced enum collection base type for source generation.

```csharp
[EnumCollection(
    CollectionName = "MyCollection",      // Required: Collection class name
    GenerateFactoryMethods = true,        // Optional: Generate factory methods
    GenerateStaticCollection = true,      // Optional: Generate static class
    Generic = false,                      // Optional: Generate generic collection
    UseSingletonInstances = true,         // Optional: Use singleton pattern
    NameComparison = StringComparison.Ordinal,  // Optional: String comparison method
    IncludeReferencedAssemblies = true    // Optional: Scan referenced assemblies
)]
```

**Properties:**
- `CollectionName` (string, required): Name of the generated collection class
- `GenerateFactoryMethods` (bool): Generate factory methods for enum values. Default: `true`
- `GenerateStaticCollection` (bool): Generate static collection class. Default: `true`
- `Generic` (bool): Generate generic collection class. Default: `false`
- `UseSingletonInstances` (bool): Use singleton instances vs factory pattern. Default: `true`
- `NameComparison` (StringComparison): String comparison for lookups. Default: `Ordinal`
- `IncludeReferencedAssemblies` (bool): Scan referenced assemblies. Default: `false`

### EnumOptionAttribute

Marks a class as an option for an enhanced enum.

```csharp
[EnumOption]
public class MyOption : MyEnumBase { }
```

### EnumLookupAttribute

Marks a property to generate a lookup method.

```csharp
[EnumLookup(
    "FindByCode",          // Required: Method name
    AllowMultiple = true,  // Optional: Return multiple results
    ReturnType = typeof(IMyInterface)  // Optional: Custom return type
)]
public string Code { get; }
```

**Parameters:**
- `methodName` (string, required): Name of the generated lookup method
- `AllowMultiple` (bool): Return `ImmutableArray<T>` for multiple results. Default: `false`
- `ReturnType` (Type): Custom return type for lookup results. Default: base enum type

## How It Works

The FractalDataWorks.EnhancedEnums source generator:

1. **Scans** your code for classes marked with `[EnumCollection]`
2. **Finds** all classes marked with `[EnumOption]` that inherit from the base class
3. **Analyzes** properties marked with `[EnumLookup]` to generate optimized lookup methods
4. **Generates** static collection classes with dictionary-based lookups
5. **Creates** factory methods and singleton instances based on configuration
6. **Compiles** the generated code as part of your build process

The generated code provides:
- **Compile-time Safety**: All lookups are strongly typed
- **Runtime Efficiency**: O(1) dictionary lookups using FrozenDictionary (.NET 8+)
- **Memory Efficiency**: Singleton pattern reduces memory usage
- **Zero Dependencies**: Generated code has no runtime dependencies

## License

This library is part of the FractalDataWorks toolkit and is licensed under the Apache License 2.0.