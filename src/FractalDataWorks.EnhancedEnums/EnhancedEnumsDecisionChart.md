# FractalDataWorks Enhanced Enums - Complete Decision Chart and Options Guide

This document provides a comprehensive guide to all decisions, options, and configurations available in the FractalDataWorks Enhanced Enums system.

## Overview

The Enhanced Enums system provides a sophisticated framework for creating strongly-typed, discoverable enumerations with extensive customization options. The system supports both local and cross-assembly discovery patterns.

## Architecture Decision Tree

```
Enhanced Enum Implementation
├── Discovery Pattern
│   ├── Local Discovery (EnumCollectionAttribute)
│   │   ├── Collection-First Pattern
│   │   │   └── Collection class with generic constraint
│   │   └── Base-First Pattern
│   │       └── Base type with derived options
│   └── Global Discovery (GlobalEnumCollectionAttribute)
│       ├── Cross-Assembly Discovery
│       │   ├── Assembly Metadata Opt-In
│       │   └── Automatic Type Discovery
│       └── Plugin Architecture Support
├── Generation Strategy
│   ├── Full Generation
│   │   ├── Complete Collection Class
│   │   ├── Factory Methods
│   │   └── Static Properties
│   └── Simplified Generation
│       ├── Abstract Method Implementations
│       └── Inherits from EnumCollectionBase<T>
└── Configuration Options
    ├── Attribute-Level Configuration
    ├── Collection-Level Configuration
    └── Option-Level Configuration
```

## Attribute Reference and Decision Points

### 1. EnumCollectionAttribute (Local Discovery)

**Purpose:** Marks a class as an enhanced enum collection for local discovery within the current compilation.

#### Configuration Properties

| Property | Type | Default | Decision Impact | Options |
|----------|------|---------|-----------------|---------|
| `CollectionName` | `string?` | `null` | Collection class naming | `null` = auto-generate from base type name |
| `ReturnType` | `Type?` | `null` | Factory method return type | `null` = infer from constraints/inheritance |
| `GenerateFactoryMethods` | `bool` | `true` | Factory method generation | `true` = generate Get/Create methods, `false` = skip |
| `GenerateStaticCollection` | `bool` | `true` | Static collection generation | `true` = generate All/Items properties, `false` = skip |
| `Generic` | `bool` | `false` | Generic collection support | `true` = generic class, `false` = typed class |
| `NameComparison` | `StringComparison` | `Ordinal` | String lookup behavior | Controls case sensitivity and culture |
| `Namespace` | `string?` | `null` | Target namespace | `null` = use containing class namespace |
| `DefaultGenericReturnType` | `Type?` | `null` | Generic fallback type | Used when return type can't be inferred |
| `IncludeReferencedAssemblies` | `bool` | `false` | Cross-assembly inclusion | `true` = scan referenced assemblies |
| `UseSingletonInstances` | `bool` | `true` | Instance management | `true` = singleton pattern, `false` = factory pattern |

#### Decision Flow for Local Discovery

```
[EnumCollectionAttribute] Applied
├── Collection Name Decision
│   ├── CollectionName != null → Use specified name
│   └── CollectionName == null → Auto-generate from base type
├── Base Type Detection
│   ├── Generic Constraint Pattern
│   │   └── class MyCollection<T> where T : EnumBase
│   └── Inheritance Pattern
│       └── class MyCollection : EnumCollectionBase<EnumBase>
├── Generation Strategy Selection
│   ├── Inherits from EnumCollectionBase<T>
│   │   └── Generate: Simplified (abstract method implementations only)
│   └── No Base Inheritance
│       └── Generate: Full collection class
└── Return Type Resolution
    ├── ReturnType specified → Use specified type
    ├── Generic constraint → Use constraint type
    ├── Inheritance → Use base generic type
    └── Fallback → Use DefaultGenericReturnType or object
```

### 2. GlobalEnumCollectionAttribute (Cross-Assembly Discovery)

**Purpose:** Marks an enhanced enum collection for global discovery across assembly boundaries.

#### Configuration Properties

| Property | Type | Default | Decision Impact | Options |
|----------|------|---------|-----------------|---------|
| `CollectionName` | `string?` | `null` | Collection class naming | `null` = auto-generate from attribute target |
| `GenerateFactoryMethods` | `bool` | `true` | Factory method generation | Controls method generation |
| `GenerateStaticCollection` | `bool` | `true` | Static collection generation | Controls property generation |
| `NameComparison` | `StringComparison` | `OrdinalIgnoreCase` | String lookup behavior | Different default than local |
| `UseSingletonInstances` | `bool` | `true` | Instance management | Singleton vs factory pattern |
| `ReturnType` | `Type?` | `null` | Factory return type | Type for generated methods |
| `Generic` | `bool` | `false` | Generic collection support | Generic class generation |

#### Cross-Assembly Discovery Flow

```
[GlobalEnumCollectionAttribute] Applied
├── Assembly Scanning
│   ├── Current Assembly → Always included
│   └── Referenced Assemblies
│       ├── MSBuild Metadata Check
│       │   └── <FdwIncludeEnhancedEnums>true</FdwIncludeEnhancedEnums>
│       └── Assembly Attribute Check
│           └── [assembly: GlobalEnumCollection(...)]
├── Type Discovery
│   ├── Cache Check → Use cached types if available
│   └── Full Scan → Discover all enum option types
├── Collection Generation
│   ├── Aggregate all discovered types
│   ├── Apply naming conventions
│   └── Generate unified collection
```

### 3. EnumOptionAttribute (Option-Level Configuration)

**Purpose:** Configures individual enum options with custom settings.

#### Configuration Properties

| Property | Type | Default | Decision Impact | Options |
|----------|------|---------|-----------------|---------|
| `Name` | `string?` | `null` | Display name override | `null` = use class name |
| `Order` | `int` | `0` | Collection ordering | Determines position in All/Items |
| `CollectionName` | `string?` | `null` | Collection targeting | Specifies which collection to include in |
| `ReturnType` | `Type?` | `null` | Option-specific return type | Overrides collection-level return type |
| `GenerateFactoryMethod` | `bool?` | `null` | Per-option factory control | Overrides collection setting |
| `MethodName` | `string?` | `null` | Custom factory method name | `null` = auto-generate from name |

#### Option Processing Decision Flow

```
EnumOptionAttribute Processing
├── Name Resolution
│   ├── Name specified → Use custom name
│   └── Name null → Use class name
├── Collection Assignment
│   ├── CollectionName specified → Target specific collection
│   └── CollectionName null → Use default collection
├── Return Type Resolution
│   ├── Option ReturnType → Use option-specific type
│   ├── Collection ReturnType → Use collection type
│   └── Inferred Type → Use constraint/inheritance type
├── Factory Method Decision
│   ├── GenerateFactoryMethod = true → Generate method
│   ├── GenerateFactoryMethod = false → Skip method
│   └── GenerateFactoryMethod = null → Use collection setting
└── Method Naming
    ├── MethodName specified → Use custom name
    └── MethodName null → Generate from display name
```

### 4. EnumLookupAttribute (Lookup Method Generation)

**Purpose:** Generates lookup methods based on properties marked with this attribute.

#### Configuration Properties

| Property | Type | Default | Decision Impact | Options |
|----------|------|---------|-----------------|---------|
| `MethodName` | `string` | `""` | Lookup method name | `""` = auto-generate from property |
| `AllowMultiple` | `bool` | `false` | Return type behavior | `true` = IEnumerable<T>, `false` = T? |
| `ReturnType` | `Type?` | `null` | Lookup return type | Overrides inferred return type |

#### Lookup Generation Decision Flow

```
EnumLookupAttribute Processing
├── Property Analysis
│   ├── Property Type → Determines lookup key type
│   └── Property Name → Used for method naming
├── Method Name Generation
│   ├── MethodName specified → Use custom name
│   └── MethodName empty → Generate "GetBy{PropertyName}"
├── Return Type Decision
│   ├── AllowMultiple = true
│   │   ├── ReturnType specified → IEnumerable<ReturnType>
│   │   └── ReturnType null → IEnumerable<InferredType>
│   └── AllowMultiple = false
│       ├── ReturnType specified → ReturnType?
│       └── ReturnType null → InferredType?
└── Implementation Strategy
    ├── Dictionary lookup for performance
    └── Null safety handling
```

## Generation Decision Matrix

### Full vs Simplified Generation

| Condition | Generation Type | Generated Components |
|-----------|----------------|---------------------|
| Inherits from `EnumCollectionBase<T>` | Simplified | Abstract method implementations only |
| No inheritance from base | Full | Complete collection class |
| Generic collection requested | Full | Generic collection with type parameters |
| Cross-assembly discovery | Full | Unified collection across assemblies |

### Return Type Resolution Priority

| Priority | Source | Description |
|----------|--------|-------------|
| 1 | Explicit attribute `ReturnType` | Directly specified in attribute |
| 2 | Option-level `ReturnType` | Specified on `EnumOptionAttribute` |
| 3 | Collection-level `ReturnType` | Specified on collection attribute |
| 4 | Generic constraint | Extracted from `where T : BaseType` |
| 5 | Inheritance pattern | Extracted from `EnumCollectionBase<T>` |
| 6 | Default generic type | `DefaultGenericReturnType` property |
| 7 | Fallback | `object` type |

### Singleton vs Factory Pattern

| Setting | Pattern | Generated Code Example |
|---------|---------|----------------------|
| `UseSingletonInstances = true` | Singleton | `public static MyOption Instance { get; } = new();` |
| `UseSingletonInstances = false` | Factory | `public static MyOption Create() => new();` |

## Advanced Configuration Scenarios

### 1. Plugin Architecture Support

For plugin architectures, use global discovery with assembly metadata:

**MSBuild Configuration:**
```xml
<PropertyGroup>
  <FdwIncludeEnhancedEnums>true</FdwIncludeEnhancedEnums>
</PropertyGroup>
```

**Assembly-Level Declaration:**
```csharp
[assembly: GlobalEnumCollection(typeof(IPluginCommand), 
    CollectionName = "AllPluginCommands")]
```

### 2. Multi-Collection Scenarios

Different options can target different collections:

```csharp
[EnumOption(CollectionName = "SystemCommands")]
public class SaveCommand : ICommand { }

[EnumOption(CollectionName = "UserCommands")]
public class CustomCommand : ICommand { }
```

### 3. Complex Lookup Scenarios

Multiple lookup methods with different behaviors:

```csharp
public abstract class StatusOption : EnumOptionBase<StatusOption>
{
    [EnumLookup(AllowMultiple = false)]
    public abstract int Code { get; }
    
    [EnumLookup(AllowMultiple = true, MethodName = "FindByCategory")]
    public abstract string Category { get; }
}
```

## Code Generation Output Examples

### Basic Collection Generation

**Input:**
```csharp
[EnumCollection(typeof(MyEnum))]
public partial class MyEnumCollection { }

public class Option1 : MyEnum { }
public class Option2 : MyEnum { }
```

**Generated Output:**
```csharp
partial class MyEnumCollection
{
    public static IReadOnlyList<MyEnum> All { get; } = new[]
    {
        Option1.Instance,
        Option2.Instance
    };
    
    public static MyEnum GetById(string id) => /* implementation */;
    public static MyEnum GetByName(string name) => /* implementation */;
    public static bool TryGetByName(string name, out MyEnum? result) => /* implementation */;
}
```

### Simplified Generation with Base

**Input:**
```csharp
[EnumCollection(typeof(MyEnum))]
public partial class MyEnumCollection : EnumCollectionBase<MyEnum> { }
```

**Generated Output:**
```csharp
partial class MyEnumCollection
{
    protected override IEnumerable<MyEnum> GetAllItems() => new[]
    {
        Option1.Instance,
        Option2.Instance
    };
}
```

### Cross-Assembly Collection

**Input:**
```csharp
[GlobalEnumCollection(typeof(ICommand), CollectionName = "AllCommands")]
public partial class CommandRegistry { }
```

**Generated Output:**
```csharp
partial class CommandRegistry
{
    public static IReadOnlyList<ICommand> AllCommands { get; } = new[]
    {
        // Options from current assembly
        LocalCommand1.Instance,
        LocalCommand2.Instance,
        // Options from referenced assemblies
        Plugin1.Command1.Instance,
        Plugin2.Command1.Instance
    };
    
    // Factory methods and lookup methods
}
```

## Performance Considerations

### Caching Strategy

| Component | Caching Mechanism | Performance Impact |
|-----------|------------------|-------------------|
| Assembly Type Discovery | Static dictionary cache | Avoids repeated reflection |
| Generated Collections | Static readonly properties | Single initialization |
| Lookup Methods | Dictionary-based | O(1) lookup performance |
| Singleton Instances | Static properties | Memory efficient |

### .NET 8+ Optimizations

The system includes conditional compilation for .NET 8+ features:

```csharp
#if NET8_0_OR_GREATER
    private static readonly FrozenDictionary<string, T> _nameToItem = /* ... */;
    private static readonly FrozenSet<T> _allItems = /* ... */;
#else
    private static readonly Dictionary<string, T> _nameToItem = /* ... */;
    private static readonly HashSet<T> _allItems = /* ... */;
#endif
```

## Troubleshooting Guide

### Common Configuration Issues

| Issue | Symptom | Solution |
|-------|---------|----------|
| No types discovered | Empty collection generated | Check assembly metadata or attribute placement |
| Wrong return type | Compilation errors | Specify explicit `ReturnType` in attribute |
| Missing factory methods | Methods not generated | Ensure `GenerateFactoryMethods = true` |
| Duplicate method names | Compilation conflicts | Use custom `MethodName` in attributes |
| Cross-assembly not working | Local types only | Verify MSBuild metadata and assembly attributes |

### Debugging Discovery

The system provides detailed logging through the source generator infrastructure. Enable source generator logs to troubleshoot discovery issues.

## Best Practices

### 1. Naming Conventions
- Use descriptive collection names that indicate purpose
- Follow standard C# naming conventions for generated classes
- Use consistent naming patterns across related enums

### 2. Performance Optimization
- Prefer singleton instances for immutable enum options
- Use appropriate string comparison modes for your use case
- Consider generic collections for better type safety

### 3. Maintainability
- Group related enum options in the same namespace
- Use clear, descriptive names for enum options
- Document complex configuration scenarios

### 4. Plugin Architecture
- Use global discovery for cross-assembly scenarios
- Implement proper versioning for plugin compatibility
- Consider using interfaces for better abstraction

This comprehensive guide covers all aspects of the FractalDataWorks Enhanced Enums system, providing developers with the knowledge needed to effectively use and configure the system for their specific requirements.