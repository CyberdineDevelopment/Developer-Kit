# Manual Enhanced Enum Collections

This document explains how to manually create Enhanced Enum collections for Service Types and Connection Types until the source generator is available.

## Overview

The Enhanced Enum pattern requires a collection class to manage all enum instances. Until the source generator automatically creates these collections, you need to manually implement them.

## Service Type Collection

### Step 1: Define Your Service Type Base

```csharp
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.Services;

[EnumCollection(
    CollectionName = "ServiceTypes",
    ReturnType = typeof(IServiceFactory),
    GenerateFactoryMethods = true
)]
public abstract class MyServiceTypeBase : ServiceTypeBase
{
    protected MyServiceTypeBase(int id, string name, string description)
        : base(id, name, description)
    {
    }
}
```

### Step 2: Create Concrete Service Types

```csharp
[EnumOption(Order = 1)]
public class SqlServerServiceType : MyServiceTypeBase
{
    public SqlServerServiceType() 
        : base(1, "SqlServer", "SQL Server Database Service")
    {
    }
    
    public override IServiceFactory CreateFactory()
    {
        return new SqlServerServiceFactory();
    }
}

[EnumOption(Order = 2)]
public class PostgreSqlServiceType : MyServiceTypeBase
{
    public PostgreSqlServiceType() 
        : base(2, "PostgreSql", "PostgreSQL Database Service")
    {
    }
    
    public override IServiceFactory CreateFactory()
    {
        return new PostgreSqlServiceFactory();
    }
}
```

### Step 3: Manually Create the Collection Class

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

public static class ServiceTypes
{
    private static readonly Dictionary<int, MyServiceTypeBase> _byId;
    private static readonly Dictionary<string, MyServiceTypeBase> _byName;
    private static readonly List<MyServiceTypeBase> _all;
    
    // Singleton instances
    public static readonly SqlServerServiceType SqlServer = new SqlServerServiceType();
    public static readonly PostgreSqlServiceType PostgreSql = new PostgreSqlServiceType();
    
    static ServiceTypes()
    {
        // Initialize collection with all service types
        _all = new List<MyServiceTypeBase>
        {
            SqlServer,
            PostgreSql
        };
        
        // Build lookup dictionaries
        _byId = _all.ToDictionary(x => x.Id);
        _byName = _all.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
    }
    
    /// <summary>
    /// Gets all service types.
    /// </summary>
    public static IReadOnlyList<MyServiceTypeBase> All => _all.AsReadOnly();
    
    /// <summary>
    /// Gets a service type by ID.
    /// </summary>
    public static MyServiceTypeBase GetById(int id)
    {
        return _byId.TryGetValue(id, out var type) 
            ? type 
            : throw new ArgumentException($"No service type found with ID {id}");
    }
    
    /// <summary>
    /// Tries to get a service type by ID.
    /// </summary>
    public static bool TryGetById(int id, out MyServiceTypeBase serviceType)
    {
        return _byId.TryGetValue(id, out serviceType);
    }
    
    /// <summary>
    /// Gets a service type by name.
    /// </summary>
    public static MyServiceTypeBase GetByName(string name)
    {
        return _byName.TryGetValue(name, out var type) 
            ? type 
            : throw new ArgumentException($"No service type found with name '{name}'");
    }
    
    /// <summary>
    /// Tries to get a service type by name.
    /// </summary>
    public static bool TryGetByName(string name, out MyServiceTypeBase serviceType)
    {
        return _byName.TryGetValue(name, out serviceType);
    }
    
    /// <summary>
    /// Creates a service factory for the specified type.
    /// </summary>
    public static IServiceFactory CreateFactory(string typeName)
    {
        var serviceType = GetByName(typeName);
        return serviceType.CreateFactory();
    }
    
    /// <summary>
    /// Creates a service factory for the specified type.
    /// </summary>
    public static IServiceFactory CreateFactory(int typeId)
    {
        var serviceType = GetById(typeId);
        return serviceType.CreateFactory();
    }
}
```

## Connection Type Collection

### Step 1: Define Your Connection Type Base

```csharp
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.Connections;

[EnumCollection(
    CollectionName = "ConnectionTypes",
    ReturnType = typeof(IConnectionFactory),
    GenerateFactoryMethods = true
)]
public abstract class MyConnectionTypeBase : ConnectionTypeBase
{
    protected MyConnectionTypeBase(int id, string name, string description)
        : base(id, name, description)
    {
    }
}
```

### Step 2: Create Concrete Connection Types

```csharp
[EnumOption(Order = 1)]
public class SqlServerConnectionType : MyConnectionTypeBase
{
    public SqlServerConnectionType() 
        : base(1, "SqlServer", "SQL Server Database Connection")
    {
    }
    
    public override IConnectionFactory CreateFactory()
    {
        return new SqlServerConnectionFactory();
    }
}

[EnumOption(Order = 2)]
public class FileConnectionType : MyConnectionTypeBase
{
    public FileConnectionType() 
        : base(2, "File", "File System Connection")
    {
    }
    
    public override IConnectionFactory CreateFactory()
    {
        return new FileConnectionFactory();
    }
}
```

### Step 3: Manually Create the Collection Class

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

public static class ConnectionTypes
{
    private static readonly Dictionary<int, MyConnectionTypeBase> _byId;
    private static readonly Dictionary<string, MyConnectionTypeBase> _byName;
    private static readonly List<MyConnectionTypeBase> _all;
    
    // Singleton instances
    public static readonly SqlServerConnectionType SqlServer = new SqlServerConnectionType();
    public static readonly FileConnectionType File = new FileConnectionType();
    
    static ConnectionTypes()
    {
        // Initialize collection with all connection types
        _all = new List<MyConnectionTypeBase>
        {
            SqlServer,
            File
        };
        
        // Build lookup dictionaries
        _byId = _all.ToDictionary(x => x.Id);
        _byName = _all.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
    }
    
    /// <summary>
    /// Gets all connection types.
    /// </summary>
    public static IReadOnlyList<MyConnectionTypeBase> All => _all.AsReadOnly();
    
    /// <summary>
    /// Gets a connection type by ID.
    /// </summary>
    public static MyConnectionTypeBase GetById(int id)
    {
        return _byId.TryGetValue(id, out var type) 
            ? type 
            : throw new ArgumentException($"No connection type found with ID {id}");
    }
    
    /// <summary>
    /// Tries to get a connection type by ID.
    /// </summary>
    public static bool TryGetById(int id, out MyConnectionTypeBase connectionType)
    {
        return _byId.TryGetValue(id, out connectionType);
    }
    
    /// <summary>
    /// Gets a connection type by name.
    /// </summary>
    public static MyConnectionTypeBase GetByName(string name)
    {
        return _byName.TryGetValue(name, out var type) 
            ? type 
            : throw new ArgumentException($"No connection type found with name '{name}'");
    }
    
    /// <summary>
    /// Tries to get a connection type by name.
    /// </summary>
    public static bool TryGetByName(string name, out MyConnectionTypeBase connectionType)
    {
        return _byName.TryGetValue(name, out connectionType);
    }
    
    /// <summary>
    /// Creates a connection factory for the specified type.
    /// </summary>
    public static IConnectionFactory CreateFactory(string typeName)
    {
        var connectionType = GetByName(typeName);
        return connectionType.CreateFactory();
    }
    
    /// <summary>
    /// Creates a connection factory for the specified type.
    /// </summary>
    public static IConnectionFactory CreateFactory(int typeId)
    {
        var connectionType = GetById(typeId);
        return connectionType.CreateFactory();
    }
}
```

## Advanced Pattern: Generic Collection Base

For multiple enum collections, you can create a reusable base class:

```csharp
public abstract class EnumCollectionBase<TBase> where TBase : class
{
    private readonly Dictionary<int, TBase> _byId;
    private readonly Dictionary<string, TBase> _byName;
    private readonly List<TBase> _all;
    
    protected EnumCollectionBase(params TBase[] items)
    {
        _all = new List<TBase>(items);
        _byId = new Dictionary<int, TBase>();
        _byName = new Dictionary<string, TBase>(StringComparer.OrdinalIgnoreCase);
        
        foreach (var item in items)
        {
            if (item is IEnumOption enumOption)
            {
                _byId[enumOption.Id] = item;
                _byName[enumOption.Name] = item;
            }
        }
    }
    
    public IReadOnlyList<TBase> All => _all.AsReadOnly();
    
    public TBase GetById(int id)
    {
        return _byId.TryGetValue(id, out var type) 
            ? type 
            : throw new ArgumentException($"No item found with ID {id}");
    }
    
    public bool TryGetById(int id, out TBase item)
    {
        return _byId.TryGetValue(id, out item);
    }
    
    public TBase GetByName(string name)
    {
        return _byName.TryGetValue(name, out var type) 
            ? type 
            : throw new ArgumentException($"No item found with name '{name}'");
    }
    
    public bool TryGetByName(string name, out TBase item)
    {
        return _byName.TryGetValue(name, out item);
    }
}

// Usage
public static class ServiceTypes : EnumCollectionBase<MyServiceTypeBase>
{
    public static readonly SqlServerServiceType SqlServer = new SqlServerServiceType();
    public static readonly PostgreSqlServiceType PostgreSql = new PostgreSqlServiceType();
    
    static ServiceTypes() : base(SqlServer, PostgreSql)
    {
    }
    
    public static IServiceFactory CreateFactory(string typeName)
    {
        var serviceType = GetByName(typeName);
        return serviceType.CreateFactory();
    }
}
```

## Dependency Injection Integration

Register your enum collections with the DI container:

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServiceTypes(this IServiceCollection services)
    {
        // Register all service types as singletons
        services.AddSingleton(ServiceTypes.SqlServer);
        services.AddSingleton(ServiceTypes.PostgreSql);
        
        // Register factories
        services.AddSingleton<IServiceFactory>(provider => 
            ServiceTypes.SqlServer.CreateFactory());
        
        // Register collection itself
        services.AddSingleton<IEnumerable<MyServiceTypeBase>>(ServiceTypes.All);
        
        return services;
    }
    
    public static IServiceCollection AddConnectionTypes(this IServiceCollection services)
    {
        // Register all connection types as singletons
        services.AddSingleton(ConnectionTypes.SqlServer);
        services.AddSingleton(ConnectionTypes.File);
        
        // Register factories
        services.AddSingleton<IConnectionFactory>(provider => 
            ConnectionTypes.SqlServer.CreateFactory());
        
        // Register collection itself
        services.AddSingleton<IEnumerable<MyConnectionTypeBase>>(ConnectionTypes.All);
        
        return services;
    }
}
```

## Usage Examples

### Getting a Service Type

```csharp
// By static property
var sqlServerType = ServiceTypes.SqlServer;

// By ID
var serviceType = ServiceTypes.GetById(1);

// By name
var serviceType = ServiceTypes.GetByName("SqlServer");

// Try pattern
if (ServiceTypes.TryGetByName("PostgreSql", out var pgType))
{
    var factory = pgType.CreateFactory();
}
```

### Creating Services

```csharp
// Direct factory creation
var factory = ServiceTypes.CreateFactory("SqlServer");
var service = factory.Create(configuration);

// Using the service type
var serviceType = ServiceTypes.SqlServer;
var factory = serviceType.CreateFactory();
var service = factory.Create(configuration);
```

### Iterating All Types

```csharp
foreach (var serviceType in ServiceTypes.All)
{
    Console.WriteLine($"{serviceType.Id}: {serviceType.Name} - {serviceType.Description}");
}

// LINQ queries
var databaseTypes = ServiceTypes.All
    .Where(t => t.Name.Contains("Sql"))
    .OrderBy(t => t.Id)
    .ToList();
```

### With Dependency Injection

```csharp
public class MyService
{
    private readonly IServiceProvider _serviceProvider;
    
    public MyService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public void UseServiceType()
    {
        // Get specific type
        var sqlServerType = _serviceProvider.GetService<SqlServerServiceType>();
        
        // Get all types
        var allTypes = _serviceProvider.GetService<IEnumerable<MyServiceTypeBase>>();
    }
}
```

## Migration Path

When the source generator becomes available:

1. Keep your manual collection class temporarily
2. Add the `[GenerateCollection]` attribute to your base class
3. The generator will create a partial class with the same name
4. Remove your manual implementation
5. The generated code will have the same API surface

## Best Practices

1. **Use Singleton Pattern**: Create single instances of each enum type
2. **Thread Safety**: Use static constructors for initialization
3. **Consistent IDs**: Document and maintain ID assignments
4. **Case-Insensitive Names**: Use StringComparer.OrdinalIgnoreCase for name lookups
5. **Validation**: Add validation in constructors to prevent duplicate IDs/names
6. **Documentation**: Document each enum type's purpose and usage

## Common Pitfalls

1. **Forgetting to add new types to the collection**: Always update the static constructor
2. **ID conflicts**: Maintain a registry of assigned IDs
3. **Name conflicts**: Use consistent naming conventions
4. **Circular dependencies**: Avoid complex initialization in enum constructors
5. **Missing null checks**: Always validate inputs in lookup methods

## Testing

Example unit tests for your collection:

```csharp
[Fact]
public void ServiceTypes_All_ReturnsAllTypes()
{
    var all = ServiceTypes.All;
    
    all.Should().HaveCount(2);
    all.Should().Contain(ServiceTypes.SqlServer);
    all.Should().Contain(ServiceTypes.PostgreSql);
}

[Fact]
public void ServiceTypes_GetById_ReturnsCorrectType()
{
    var type = ServiceTypes.GetById(1);
    
    type.Should().Be(ServiceTypes.SqlServer);
    type.Name.Should().Be("SqlServer");
}

[Fact]
public void ServiceTypes_GetByName_IsCaseInsensitive()
{
    var type1 = ServiceTypes.GetByName("sqlserver");
    var type2 = ServiceTypes.GetByName("SQLSERVER");
    var type3 = ServiceTypes.GetByName("SqlServer");
    
    type1.Should().Be(ServiceTypes.SqlServer);
    type2.Should().Be(ServiceTypes.SqlServer);
    type3.Should().Be(ServiceTypes.SqlServer);
}

[Fact]
public void ServiceTypes_CreateFactory_ReturnsValidFactory()
{
    var factory = ServiceTypes.CreateFactory("SqlServer");
    
    factory.Should().NotBeNull();
    factory.Should().BeOfType<SqlServerServiceFactory>();
}
```