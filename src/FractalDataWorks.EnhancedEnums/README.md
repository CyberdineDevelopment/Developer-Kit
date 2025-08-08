# FractalDataWorks.EnhancedEnums

Core Enhanced Enum pattern base classes and attributes. Provides the foundation for type-safe enum-like implementations without requiring source generators.

## Overview

The Enhanced Enums library provides a powerful alternative to standard C# enumerations, offering:

- **Type Safety**: Strongly-typed enumerations with compile-time safety
- **Rich Metadata**: Support for IDs, names, descriptions, and custom properties
- **Extensibility**: Easy to extend with custom behavior and validation
- **Collection Support**: Automatic collection generation for enum discovery and lookup
- **Factory Patterns**: Built-in factory method generation for creating services and components
- **Framework Integration**: Seamless integration with dependency injection containers

## Key Components

### Base Classes

#### `IEnumOption` and `IEnumOption<T>`
Core interfaces that define the contract for enhanced enums:

```csharp
public interface IEnumOption
{
    int Id { get; }
    string Name { get; }
}

public interface IEnumOption<T> : IEnumOption where T : IEnumOption<T>
{
    // Self-referencing generic pattern for type safety
}
```

#### `EnumOptionBase<T>`
The primary base class for enhanced enums with constructor-based initialization:

```csharp
public abstract class EnumOptionBase<T> : IEnumOption where T : EnumOptionBase<T>
{
    public int Id { get; }
    public string Name { get; }
    
    protected EnumOptionBase(int id, string name)
    {
        Id = id;
        Name = name;
    }
}
```

#### `EnumOption<TBase>`
Alternative base class with virtual properties for more flexibility:

```csharp
public abstract class EnumOption<TBase> where TBase : EnumOption<TBase>
{
    public virtual string Name => GetType().Name;
    public virtual string DisplayName => Name;
    public virtual int Ordinal { get; protected set; }
}
```

### Attributes

#### `[EnhancedEnumBase]`
Marks a class as the base type for an enhanced enum collection:

```csharp
[EnhancedEnumBase("ServiceTypes")]
public abstract class ServiceTypeBase : EnumOptionBase<ServiceTypeBase>
{
    protected ServiceTypeBase(int id, string name) : base(id, name) { }
}
```

#### `[EnumCollection]`
Configures collection generation for enhanced enums:

```csharp
[EnumCollection(
    CollectionName = "ServiceTypes",
    ReturnType = typeof(IServiceType),
    GenerateFactoryMethods = true,
    GenerateStaticCollection = true
)]
public abstract class ServiceTypeBase : EnumOptionBase<ServiceTypeBase> { }
```

**Properties:**
- `CollectionName`: Name of the generated collection class
- `ReturnType`: Interface or base type returned by factory methods
- `GenerateFactoryMethods`: Whether to generate factory methods
- `GenerateStaticCollection`: Generate static vs instance collection
- `Generic`: Generate generic collection class
- `NameComparison`: String comparison for name lookups
- `UseSingletonInstances`: Use singleton pattern for enum instances

#### `[EnumOption]`
Marks individual enum implementations and configures their behavior:

```csharp
[EnumOption(Name = "SQL Server", Order = 1)]
public class SqlServerServiceType : ServiceTypeBase
{
    public SqlServerServiceType() : base(1, "SqlServer") { }
}
```

**Properties:**
- `Name`: Custom display name
- `Order`: Sort order in collections
- `CollectionName`: Specific collection to include in
- `ReturnType`: Override return type for this option
- `GenerateFactoryMethod`: Override factory method generation
- `MethodName`: Custom factory method name

## Usage Examples

### Basic Enhanced Enum

```csharp
// Define the base type
[EnumCollection(CollectionName = "ConnectionTypes")]
public abstract class ConnectionTypeBase : EnumOptionBase<ConnectionTypeBase>
{
    protected ConnectionTypeBase(int id, string name) : base(id, name) { }
}

// Define concrete implementations
[EnumOption]
public class SqlServerConnectionType : ConnectionTypeBase
{
    public SqlServerConnectionType() : base(1, "SqlServer") { }
}

[EnumOption]
public class PostgreSqlConnectionType : ConnectionTypeBase
{
    public PostgreSqlConnectionType() : base(2, "PostgreSql") { }
}

// Usage
var connectionType = new SqlServerConnectionType();
Console.WriteLine($"ID: {connectionType.Id}, Name: {connectionType.Name}");
// Output: ID: 1, Name: SqlServer
```

### Service Factory Pattern

Enhanced enums integrate seamlessly with service factories:

```csharp
public interface IDataService
{
    Task<IFdwResult<T>> ExecuteQuery<T>(string query);
}

[EnumCollection(
    CollectionName = "DataServiceTypes",
    ReturnType = typeof(IDataService),
    GenerateFactoryMethods = true
)]
public abstract class DataServiceTypeBase : EnumOptionBase<DataServiceTypeBase>
{
    protected DataServiceTypeBase(int id, string name) : base(id, name) { }
    
    public abstract IServiceFactory<IDataService> CreateFactory();
}

[EnumOption]
public class SqlServerDataServiceType : DataServiceTypeBase
{
    public SqlServerDataServiceType() : base(1, "SqlServer") { }
    
    public override IServiceFactory<IDataService> CreateFactory()
    {
        return new SqlServerDataServiceFactory();
    }
}
```

### Complex Configuration with Multiple Collections

```csharp
[EnumCollection(CollectionName = "AllProviders", ReturnType = typeof(IProvider))]
[EnumCollection(CollectionName = "DatabaseProviders", ReturnType = typeof(IDatabaseProvider))]
[EnumCollection(CollectionName = "CloudProviders", ReturnType = typeof(ICloudProvider))]
public abstract class ProviderTypeBase : EnumOptionBase<ProviderTypeBase>
{
    protected ProviderTypeBase(int id, string name) : base(id, name) { }
}

[EnumOption(CollectionName = "DatabaseProviders", Order = 1)]
public class SqlServerProvider : ProviderTypeBase, IDatabaseProvider
{
    public SqlServerProvider() : base(1, "SqlServer") { }
}

[EnumOption(CollectionName = "CloudProviders", Order = 2)]
public class AzureProvider : ProviderTypeBase, ICloudProvider
{
    public AzureProvider() : base(2, "Azure") { }
}
```

### Integration with Dependency Injection

Enhanced enums work seamlessly with DI containers:

```csharp
// In your startup/configuration
services.AddServiceTypes(Assembly.GetExecutingAssembly());

// The framework automatically registers:
// - All [EnumOption] classes as singleton services
// - Factory interfaces for service creation
// - Collection classes for enum discovery

// Usage in your services
public class MyService
{
    private readonly IServiceProvider _serviceProvider;
    
    public MyService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public void ProcessConnectionType(string typeName)
    {
        // Get specific enum option
        var connectionType = _serviceProvider.GetService<SqlServerConnectionType>();
        
        // Or get factory for creating services
        var factory = _serviceProvider.GetService<IServiceFactory<IDataService>>();
    }
}
```

## Architecture Benefits

### Type Safety
Enhanced enums provide compile-time type safety while maintaining the flexibility of classes:

```csharp
// Compiler enforces type safety
public void ProcessConnectionType(ConnectionTypeBase connectionType)
{
    // This method only accepts ConnectionTypeBase implementations
    // Compile-time error if you pass wrong type
}

// Usage
ProcessConnectionType(new SqlServerConnectionType()); // ✓ Valid
ProcessConnectionType(new ServiceTypeBase()); // ✗ Compile error
```

### Extensibility
Unlike standard enums, enhanced enums can be extended with custom behavior:

```csharp
[EnumOption]
public class SqlServerConnectionType : ConnectionTypeBase
{
    public SqlServerConnectionType() : base(1, "SqlServer") { }
    
    // Custom behavior specific to SQL Server
    public string GetConnectionString(string server, string database)
    {
        return $"Server={server};Database={database};";
    }
    
    // Custom validation
    public bool IsValidConnectionString(string connectionString)
    {
        return connectionString.Contains("Server=");
    }
}
```

### Framework Integration
Enhanced enums integrate naturally with the FractalDataWorks framework patterns:

- **ServiceBase Integration**: Enhanced enums can serve as service type discriminators
- **Configuration Integration**: Enums can carry configuration metadata
- **Validation Integration**: Custom validation logic in enum implementations
- **Logging Integration**: Automatic logging of enum usage and factory creation

## Best Practices

### Naming Conventions
- Use descriptive names that clearly indicate the enum's purpose
- Suffix base classes with "Base" or "Type"
- Use PascalCase for enum option class names
- Keep collection names plural (e.g., "ServiceTypes", "ConnectionTypes")

### ID Management
- Use consistent ID numbering schemes
- Reserve ranges for different categories (e.g., 1-100 for database types)
- Document ID assignments to avoid conflicts
- Consider using meaningful IDs rather than sequential numbers

### Collection Organization
- Group related enums into logical collections
- Use descriptive collection names
- Consider multiple collections for different use cases
- Leverage return type interfaces for polymorphism

### Performance Considerations
- Use `UseSingletonInstances = true` (default) for better memory usage
- Consider lazy initialization for expensive enum options
- Cache frequently accessed enum collections
- Minimize reflection usage in custom implementations

## Advanced Scenarios

### Custom Validation
```csharp
[EnumOption]
public class DatabaseConnectionType : ConnectionTypeBase, IValidatable
{
    public DatabaseConnectionType() : base(1, "Database") { }
    
    public IValidationResult Validate()
    {
        var result = new ValidationResult();
        
        if (string.IsNullOrEmpty(Name))
        {
            result.AddError("Name is required");
        }
        
        return result;
    }
}
```

### Runtime Discovery
```csharp
// Discover all enum options at runtime
var allConnectionTypes = Assembly.GetExecutingAssembly()
    .GetTypes()
    .Where(t => typeof(ConnectionTypeBase).IsAssignableFrom(t) 
        && !t.IsAbstract
        && t.GetCustomAttribute<EnumOptionAttribute>() != null)
    .Select(t => (ConnectionTypeBase)Activator.CreateInstance(t))
    .ToList();
```

## Migration from Standard Enums

When migrating from standard C# enums to enhanced enums:

### Before (Standard Enum)
```csharp
public enum ConnectionType
{
    SqlServer = 1,
    PostgreSql = 2,
    MySql = 3
}

// Limited functionality
public void ProcessConnection(ConnectionType type)
{
    switch (type)
    {
        case ConnectionType.SqlServer:
            // Handle SQL Server
            break;
        // ...
    }
}
```

### After (Enhanced Enum)
```csharp
[EnumCollection(CollectionName = "ConnectionTypes")]
public abstract class ConnectionTypeBase : EnumOptionBase<ConnectionTypeBase>
{
    protected ConnectionTypeBase(int id, string name) : base(id, name) { }
    
    public abstract IConnection CreateConnection(string connectionString);
}

[EnumOption]
public class SqlServerConnectionType : ConnectionTypeBase
{
    public SqlServerConnectionType() : base(1, "SqlServer") { }
    
    public override IConnection CreateConnection(string connectionString)
    {
        return new SqlServerConnection(connectionString);
    }
}

// Rich functionality with polymorphism
public void ProcessConnection(ConnectionTypeBase type, string connectionString)
{
    var connection = type.CreateConnection(connectionString);
    // Type-specific behavior handled by the enum implementation
}
```

## Troubleshooting

### Common Issues

**Issue**: Enum options not being discovered
**Solution**: Ensure classes are marked with `[EnumOption]` attribute and inherit from the correct base class

**Issue**: Factory methods not generated  
**Solution**: Set `GenerateFactoryMethods = true` in `[EnumCollection]` attribute

**Issue**: Dependency injection not working
**Solution**: Call `services.AddServiceTypes(assembly)` in your service configuration

**Issue**: Circular dependencies
**Solution**: Use factory patterns instead of direct service injection in enum constructors

## Related Documentation

- [ServiceBase Usage Patterns](../FractalDataWorks.Services/README.md#servicebase-patterns)
- [Connection Abstractions](../FractalDataWorks.Connections/README.md)
- [Framework Architecture Overview](../../README.md#architecture)
- [Dependency Injection Integration](../FractalDataWorks.DependencyInjection/README.md)