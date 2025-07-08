# Provider Strategy Pattern Architecture

## Overview

The Provider Strategy Pattern uses Enhanced Enums to create a plugin-based architecture where different providers can implement the same high-level operations (Query, Insert, Send, etc.) in provider-specific ways. This pattern separates the "what" (commands) from the "how" (provider implementation).

## Core Concepts

### ProviderStrategyEnum Base Pattern

The ProviderStrategyEnum pattern consists of three layers:

1. **Connection Type** - Defines what operations are available (DataConnection, NotificationConnection, etc.)
2. **Command Type** - Defines the specific operations (Query, Insert, Send, etc.)
3. **Provider Strategy** - Defines how to execute those operations (SqlServer, FileSystem, Email, etc.)

```csharp
// Layer 1: Connection Type defines available operations
[EnhancedEnum("ConnectionTypes", IncludeReferencedAssemblies = true)]
public abstract class ConnectionTypeEnum : IEnhancedEnum<ConnectionTypeEnum>
{
    public abstract string Name { get; }
    public abstract Type CommandEnumType { get; }
    public abstract Type ParserInterfaceType { get; }
    public abstract Type ConnectionInterfaceType { get; }
    
    // Factory method for creating connections
    public abstract IConnection CreateConnection(
        IServiceProvider serviceProvider,
        ConnectionConfiguration configuration);
}

// Layer 2: Commands available for each connection type
public enum DataCommandType
{
    Query,      // Read operations
    Insert,     // Create new records
    Update,     // Modify existing records
    Upsert,     // Create or update
    Delete,     // Remove records
    Find        // Get by identity
}

// Layer 3: Provider-specific implementations
[EnhancedEnum("DataProviders", IncludeReferencedAssemblies = true)]
public abstract class DataProviderStrategyEnum : IEnhancedEnum<DataProviderStrategyEnum>
{
    public abstract string Name { get; }
    public abstract Type ParserType { get; }
    public abstract Type AdapterType { get; }
    public abstract DataStoreCapabilities Capabilities { get; }
    
    // Validates provider-specific configuration
    public abstract Result<Unit> ValidateConfiguration(Dictionary<string, object> datum);
}
```

## Configuration-Driven Architecture

### Configuration Structure

Configuration drives everything in this pattern. Each connection has:

```csharp
public class ConnectionConfiguration : ConfigurationBase<ConnectionConfiguration>
{
    // Identity
    public string Name { get; set; }
    public string ConnectionTypeName { get; set; } // Maps to ConnectionTypeEnum.Name
    public string ProviderName { get; set; } // Maps to ProviderStrategyEnum.Name
    
    // Provider-specific settings
    public Dictionary<string, object> Datum { get; set; } = new();
    
    // Schema information
    public List<DataContainerDefinition> Containers { get; set; } = new();
    
    // Common settings
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public int RetryCount { get; set; } = 3;
    public bool EnableLogging { get; set; } = true;
    
    public override bool IsValid =>
        !string.IsNullOrWhiteSpace(Name) &&
        !string.IsNullOrWhiteSpace(ConnectionTypeName) &&
        !string.IsNullOrWhiteSpace(ProviderName) &&
        Timeout > TimeSpan.Zero;
}
```

### Configuration Examples

```json
{
  "Connections": {
    "MainDatabase": {
      "ConnectionTypeName": "DataConnection",
      "ProviderName": "SqlServer",
      "Datum": {
        "ConnectionString": "Server=localhost;Database=MyApp;Integrated Security=true;",
        "Schema": "dbo",
        "CommandTimeout": 30
      },
      "Containers": [
        {
          "Name": "Customers",
          "ContainerType": "Table",
          "IdentityFields": ["CustomerId"],
          "Fields": [
            { "Name": "CustomerId", "DataType": "int", "Classification": "Identity" },
            { "Name": "Name", "DataType": "string", "Classification": "Attribute" },
            { "Name": "Revenue", "DataType": "decimal", "Classification": "Measure" }
          ]
        }
      ]
    },
    "FileStorage": {
      "ConnectionTypeName": "DataConnection",
      "ProviderName": "FileSystem",
      "Datum": {
        "BasePath": "C:\\Data\\Files",
        "Format": "JSON",
        "FileExtension": ".json"
      }
    },
    "EmailService": {
      "ConnectionTypeName": "NotificationConnection",
      "ProviderName": "SendGrid",
      "Datum": {
        "ApiKey": "SG.xxxxx",
        "FromAddress": "noreply@example.com",
        "FromName": "My Application"
      }
    }
  }
}
```

## Building Parsers

### Parser Interface Hierarchy

```csharp
// Base parser for all command types
public interface ICommandParser<TCommand, TParsedCommand>
    where TCommand : IConnectionCommand
    where TParsedCommand : IParsedCommand
{
    bool CanHandle(TCommand command);
    Task<Result<TParsedCommand>> ParseAsync(TCommand command, DataContainerDefinition schema);
}

// Specialized for data operations
public interface IDataCommandParser : ICommandParser<DataCommand, IParsedDataCommand>
{
    // Additional data-specific methods if needed
}

// Parsed command contains provider-specific query
public interface IParsedDataCommand : IParsedCommand
{
    string NativeQuery { get; }
    Dictionary<string, object> Parameters { get; }
    CommandMetadata Metadata { get; }
}
```

### SQL Server Parser Implementation

```csharp
public class SqlServerCommandParser : IDataCommandParser
{
    private readonly SqlExpressionTranslator _expressionTranslator;
    
    public SqlServerCommandParser()
    {
        _expressionTranslator = new SqlExpressionTranslator();
    }
    
    public bool CanHandle(DataCommand command) => true;
    
    public async Task<Result<IParsedDataCommand>> ParseAsync(
        DataCommand command, 
        DataContainerDefinition schema)
    {
        try
        {
            var parsed = command.CommandType switch
            {
                DataCommandType.Query => ParseQuery(command, schema),
                DataCommandType.Insert => ParseInsert(command, schema),
                DataCommandType.Update => ParseUpdate(command, schema),
                DataCommandType.Upsert => ParseUpsert(command, schema),
                DataCommandType.Delete => ParseDelete(command, schema),
                DataCommandType.Find => ParseFind(command, schema),
                _ => throw new NotSupportedException($"Command type {command.CommandType} not supported")
            };
            
            return Result<IParsedDataCommand>.Ok(parsed);
        }
        catch (Exception ex)
        {
            return Result<IParsedDataCommand>.Fail($"Failed to parse command: {ex.Message}");
        }
    }
    
    private IParsedDataCommand ParseQuery(DataCommand command, DataContainerDefinition schema)
    {
        var whereClause = "";
        var parameters = new Dictionary<string, object>();
        
        if (command.Predicate != null)
        {
            var translation = _expressionTranslator.Translate(command.Predicate);
            whereClause = $" WHERE {translation.SqlText}";
            parameters = translation.Parameters;
        }
        
        var sql = $"SELECT * FROM [{schema.SchemaName}].[{schema.Name}]{whereClause}";
        
        if (command.OrderBy?.Any() == true)
        {
            sql += " ORDER BY " + string.Join(", ", 
                command.OrderBy.Select(o => $"[{o.FieldName}] {(o.Descending ? "DESC" : "ASC")}"));
        }
        
        if (command.Skip.HasValue || command.Take.HasValue)
        {
            sql += $" OFFSET {command.Skip ?? 0} ROWS";
            if (command.Take.HasValue)
                sql += $" FETCH NEXT {command.Take.Value} ROWS ONLY";
        }
        
        return new SqlParsedCommand
        {
            NativeQuery = sql,
            Parameters = parameters,
            Metadata = new CommandMetadata
            {
                CommandType = DataCommandType.Query,
                ContainerName = schema.Name
            }
        };
    }
    
    private IParsedDataCommand ParseInsert(DataCommand command, DataContainerDefinition schema)
    {
        var fields = command.Record.GetAllFields();
        var fieldNames = string.Join(", ", fields.Keys.Select(k => $"[{k}]"));
        var paramNames = string.Join(", ", fields.Keys.Select(k => $"@{k}"));
        
        var sql = $"INSERT INTO [{schema.SchemaName}].[{schema.Name}] ({fieldNames}) " +
                  $"OUTPUT INSERTED.* VALUES ({paramNames})";
        
        return new SqlParsedCommand
        {
            NativeQuery = sql,
            Parameters = fields,
            Metadata = new CommandMetadata
            {
                CommandType = DataCommandType.Insert,
                ContainerName = schema.Name
            }
        };
    }
}
```

### Expression Translation

```csharp
public class SqlExpressionTranslator : ExpressionVisitor
{
    private readonly StringBuilder _sql = new();
    private readonly Dictionary<string, object> _parameters = new();
    private int _parameterIndex = 0;
    
    public TranslationResult Translate(Expression<Func<IDataRecord, bool>> predicate)
    {
        Visit(predicate.Body);
        return new TranslationResult
        {
            SqlText = _sql.ToString(),
            Parameters = _parameters
        };
    }
    
    protected override Expression VisitBinary(BinaryExpression node)
    {
        _sql.Append("(");
        Visit(node.Left);
        
        var op = node.NodeType switch
        {
            ExpressionType.Equal => " = ",
            ExpressionType.NotEqual => " <> ",
            ExpressionType.GreaterThan => " > ",
            ExpressionType.GreaterThanOrEqual => " >= ",
            ExpressionType.LessThan => " < ",
            ExpressionType.LessThanOrEqual => " <= ",
            ExpressionType.AndAlso => " AND ",
            ExpressionType.OrElse => " OR ",
            _ => throw new NotSupportedException($"Operator {node.NodeType} not supported")
        };
        
        _sql.Append(op);
        Visit(node.Right);
        _sql.Append(")");
        
        return node;
    }
    
    protected override Expression VisitMember(MemberExpression node)
    {
        // Handle nested property access like record.Attributes["CustomerType"]
        if (node.Expression is ParameterExpression)
        {
            _sql.Append($"[{node.Member.Name}]");
        }
        else if (node.Expression is MemberExpression parent && parent.Member.Name == "Attributes")
        {
            // This is accessing an attribute
            _sql.Append($"[{GetIndexerKey(node)}]");
        }
        
        return node;
    }
    
    protected override Expression VisitConstant(ConstantExpression node)
    {
        var paramName = $"@p{_parameterIndex++}";
        _sql.Append(paramName);
        _parameters[paramName] = node.Value;
        return node;
    }
}
```

## File System Parser Implementation

```csharp
public class FileSystemCommandParser : IDataCommandParser
{
    public async Task<Result<IParsedDataCommand>> ParseAsync(
        DataCommand command,
        DataContainerDefinition schema)
    {
        // For file systems, we don't translate to a query language
        // Instead, we prepare instructions for the adapter
        return command.CommandType switch
        {
            DataCommandType.Query => Result<IParsedDataCommand>.Ok(new FileSystemParsedCommand
            {
                Operation = FileOperation.Read,
                FileName = schema.Name,
                Filter = command.Predicate, // Will be applied in-memory
                Metadata = new CommandMetadata
                {
                    CommandType = DataCommandType.Query,
                    ContainerName = schema.Name
                }
            }),
            
            DataCommandType.Insert => Result<IParsedDataCommand>.Ok(new FileSystemParsedCommand
            {
                Operation = FileOperation.Append,
                FileName = schema.Name,
                Record = command.Record,
                Metadata = new CommandMetadata
                {
                    CommandType = DataCommandType.Insert,
                    ContainerName = schema.Name
                }
            }),
            
            _ => Result<IParsedDataCommand>.Fail($"Operation {command.CommandType} not supported for files")
        };
    }
}
```

## Registration and Discovery

### Auto-Registration Pattern

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFractalDataWorksConnections(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register all connection types
        foreach (var connectionType in ConnectionTypes.All)
        {
            services.AddScoped(connectionType.ConnectionInterfaceType, provider =>
            {
                var config = configuration.GetSection($"Connections:{connectionType.Name}").Get<ConnectionConfiguration>();
                return connectionType.CreateConnection(provider, config);
            });
        }
        
        // Register all providers and their parsers
        foreach (var provider in DataProviders.All)
        {
            services.AddScoped(provider.ParserType);
            services.AddScoped(provider.AdapterType);
        }
        
        // Register connection factory
        services.AddSingleton<IConnectionFactory, ConnectionFactory>();
        
        return services;
    }
}
```

### Connection Factory

```csharp
public class ConnectionFactory : IConnectionFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly Dictionary<string, Lazy<IConnection>> _connections = new();
    
    public ConnectionFactory(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        InitializeConnections();
    }
    
    private void InitializeConnections()
    {
        var connectionsConfig = _configuration.GetSection("Connections").GetChildren();
        
        foreach (var connConfig in connectionsConfig)
        {
            var config = connConfig.Get<ConnectionConfiguration>();
            var connectionName = connConfig.Key;
            
            _connections[connectionName] = new Lazy<IConnection>(() => CreateConnection(config));
        }
    }
    
    private IConnection CreateConnection(ConnectionConfiguration config)
    {
        // Find connection type
        var connectionType = ConnectionTypes.All
            .FirstOrDefault(ct => ct.Name == config.ConnectionTypeName)
            ?? throw new InvalidOperationException($"Unknown connection type: {config.ConnectionTypeName}");
        
        // Find provider
        var provider = DataProviders.All
            .FirstOrDefault(p => p.Name == config.ProviderName)
            ?? throw new InvalidOperationException($"Unknown provider: {config.ProviderName}");
        
        // Validate provider configuration
        var validationResult = provider.ValidateConfiguration(config.Datum);
        if (validationResult.IsFailure)
            throw new InvalidOperationException($"Invalid configuration: {validationResult.Error}");
        
        // Create connection
        return connectionType.CreateConnection(_serviceProvider, config);
    }
    
    public T GetConnection<T>(string name) where T : IConnection
    {
        if (!_connections.TryGetValue(name, out var lazy))
            throw new KeyNotFoundException($"Connection '{name}' not found");
            
        var connection = lazy.Value;
        if (connection is not T typed)
            throw new InvalidCastException($"Connection '{name}' is not of type {typeof(T).Name}");
            
        return typed;
    }
}
```

## Usage Examples

### Basic Usage

```csharp
public class CustomerService
{
    private readonly IConnectionFactory _connectionFactory;
    
    public CustomerService(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }
    
    public async Task<Result<Customer>> GetCustomerAsync(int customerId)
    {
        // Get data connection - works with any provider (SQL, Files, API)
        var connection = _connectionFactory.GetConnection<IDataConnection>("MainDatabase");
        
        // Use same query syntax regardless of provider
        var result = await connection.FindAsync(
            "Customers",
            new RecordIdentity { KeyValues = { ["CustomerId"] = customerId } });
            
        return result.Map(record => MapToCustomer(record));
    }
    
    public async Task<Result<Unit>> NotifyCustomerAsync(Customer customer, string message)
    {
        // Get notification connection - could be Email, SMS, etc.
        var connection = _connectionFactory.GetConnection<INotificationConnection>("EmailService");
        
        // Send uses same interface regardless of provider
        await connection.SendAsync(
            new NotificationContent { Subject = "Important Update", Body = message },
            new List<string> { customer.Email });
    }
}
```

### Advanced Query Example

```csharp
public async Task<Result<IEnumerable<Customer>>> GetHighValueCustomersAsync()
{
    var connection = _connectionFactory.GetConnection<IDataConnection>("MainDatabase");
    
    // Complex query works with any provider that supports it
    var command = new DataCommand
    {
        CommandType = DataCommandType.Query,
        TargetName = "Customers",
        Predicate = c => c.Properties["Revenue"].AsDecimal() > 100000 &&
                        c.Attributes["Status"].ToString() == "Active",
        OrderBy = new List<OrderByClause>
        {
            new() { FieldName = "Revenue", Descending = true }
        },
        Take = 100
    };
    
    var result = await connection.ExecuteAsync(command);
    return result.Map(r => r.Records.Select(MapToCustomer));
}
```

## Extending the Pattern

### Adding a New Connection Type

1. Define the connection type:
```csharp
[EnumOption(Name = "CacheConnection", Order = 3)]
public class CacheConnectionType : ConnectionTypeEnum
{
    public override string Name => "CacheConnection";
    public override Type CommandEnumType => typeof(CacheCommandType);
    public override Type ParserInterfaceType => typeof(ICacheCommandParser);
    public override Type ConnectionInterfaceType => typeof(ICacheConnection);
}

public enum CacheCommandType
{
    Get, Set, Delete, Expire, Increment, Decrement
}
```

2. Define the connection interface:
```csharp
public interface ICacheConnection : IConnection
{
    Task<Result<CacheEntry>> GetAsync(string key);
    Task<Result<Unit>> SetAsync(string key, object value, TimeSpan? ttl = null);
    Task<Result<bool>> DeleteAsync(string key);
}
```

### Adding a New Provider

1. Define the provider strategy:
```csharp
[EnumOption(Name = "Redis", Order = 1)]
public class RedisProviderStrategy : CacheProviderStrategyEnum
{
    public override string Name => "Redis";
    public override Type ParserType => typeof(RedisCommandParser);
    public override Type AdapterType => typeof(RedisAdapter);
    
    public override Result<Unit> ValidateConfiguration(Dictionary<string, object> datum)
    {
        if (!datum.TryGetValue("ConnectionString", out _))
            return Result<Unit>.Fail("Redis connection string required");
        return Result<Unit>.Ok(Unit.Value);
    }
}
```

2. Implement the parser:
```csharp
public class RedisCommandParser : ICacheCommandParser
{
    public async Task<Result<IParsedCacheCommand>> ParseAsync(
        CacheCommand command,
        CacheConfiguration config)
    {
        return command.CommandType switch
        {
            CacheCommandType.Get => new RedisParsedCommand { RedisCommand = $"GET {command.Key}" },
            CacheCommandType.Set => new RedisParsedCommand 
            { 
                RedisCommand = $"SET {command.Key} {command.Value}",
                Expiry = command.TimeToLive
            },
            _ => throw new NotSupportedException()
        };
    }
}
```

## Best Practices

1. **Provider Independence**: Write business logic using the connection interfaces, not specific providers
2. **Configuration Validation**: Always validate provider-specific configuration in the ProviderStrategyEnum
3. **Capability Checking**: Check provider capabilities before attempting operations
4. **Graceful Degradation**: Handle cases where providers don't support certain operations
5. **Async All the Way**: Use async/await consistently through the stack
6. **Result Pattern**: Use Result<T> for all operations that can fail

## Summary

The Provider Strategy Pattern enables:
- **Consistent interfaces** across different providers
- **Configuration-driven** provider selection
- **Plugin architecture** where new providers can be added without changing core code
- **Type safety** through Enhanced Enums
- **Clear separation** between what operations do and how they're implemented

This pattern is the foundation for the FractalDataWorks SDK's universal data access layer, allowing the same code to work with databases, files, APIs, and any other data source.