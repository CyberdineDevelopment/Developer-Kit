# FractalDataWorks.Services.Connections.Data

Universal data connection implementations that provide a consistent LINQ interface across any data source. Write your data access code once and run it against SQL Server, JSON files, REST APIs, or any provider.

## 📦 Package Information

- **Package ID**: `FractalDataWorks.Services.Connections.Data`
- **Target Framework**: .NET Standard 2.1
- **Dependencies**: 
  - `FractalDataWorks` (core)
  - `FractalDataWorks.Services.Connections`
  - `FractalDataWorks.Data`
- **License**: Apache 2.0

## 🎯 Purpose

This package implements the universal data access layer that enables:

- **Provider-Agnostic Data Access**: Same LINQ code works with any data source
- **Automatic Provider Discovery**: Enhanced Enums discover and register providers
- **Parser/Adapter Pattern**: Universal operations translated to provider-specific formats
- **Configuration-Driven Selection**: Switch providers through configuration changes
- **Type-Safe Operations**: Strong typing prevents runtime errors

## 🚀 Usage

### Install Package

```bash
dotnet add package FractalDataWorks.Services.Connections.Data
```

### Basic Data Access

```csharp
public class CustomerService
{
    private readonly IDataConnection _dataConnection;
    
    public CustomerService(IDataConnection dataConnection)
    {
        _dataConnection = dataConnection;
    }
    
    // Same code works with any provider
    public async Task<Result<IEnumerable<Customer>>> GetActiveCustomers()
    {
        return await _dataConnection.Query<Customer>(c => c.IsActive);
    }
    
    public async Task<Result<Customer>> GetCustomerById(string id)
    {
        return await _dataConnection.Single<Customer>(c => c.Id == id);
    }
    
    public async Task<Result<Customer>> CreateCustomer(Customer customer)
    {
        return await _dataConnection.Insert(customer);
    }
    
    public async Task<Result<int>> UpdateCustomerRegion(string customerId, string newRegion)
    {
        return await _dataConnection.Update<Customer>(
            where: c => c.Id == customerId,
            update: c => new Customer { Region = newRegion }
        );
    }
    
    public async Task<Result<int>> DeactivateInactiveCustomers()
    {
        return await _dataConnection.Update<Customer>(
            where: c => c.TotalValue == 0 && c.IsActive,
            update: c => new Customer { IsActive = false }
        );
    }
}
```

### Complex Queries

```csharp
public async Task<Result<IEnumerable<Customer>>> GetHighValueCustomersInRegion(
    string region, 
    decimal minValue)
{
    return await _dataConnection.Query<Customer>(c => 
        c.Region == region && 
        c.TotalValue >= minValue && 
        c.IsActive &&
        !c.IsDeleted);
}

public async Task<Result<IEnumerable<Customer>>> GetTopCustomersByValue(int count)
{
    // Note: OrderBy/Take support depends on provider capabilities
    var allCustomers = await _dataConnection.Query<Customer>(c => c.IsActive);
    
    if (allCustomers.IsFailure)
        return allCustomers;
        
    var topCustomers = allCustomers.Value
        .OrderByDescending(c => c.TotalValue)
        .Take(count);
        
    return Result<IEnumerable<Customer>>.Ok(topCustomers);
}
```

## 🏗️ Architecture

### Universal Data Connection

```csharp
public class DataConnection : IDataConnection
{
    private readonly IOperationParser _parser;
    private readonly IConnectionAdapter _adapter;
    private readonly DataConnectionConfiguration _configuration;
    
    public async Task<Result<IEnumerable<T>>> Query<T>(
        Expression<Func<T, bool>> predicate, 
        CancellationToken cancellationToken = default) where T : class
    {
        // 1. Create universal operation
        var operation = new QueryOperation<T>
        {
            OperationId = Guid.NewGuid().ToString(),
            OperationType = OperationType.Query,
            WhereClause = predicate,
            TargetName = typeof(T).Name,
            CreatedAt = DateTime.UtcNow,
            Context = new Dictionary<string, object>()
        };
        
        // 2. Parse to provider-specific format
        var schema = GetContainerDefinition<T>();
        var parseResult = await _parser.Parse(operation, schema);
        if (parseResult.IsFailure)
            return Result<IEnumerable<T>>.Fail(parseResult.Error);
        
        // 3. Execute via adapter
        return await _adapter.Execute<IEnumerable<T>>(parseResult.Value, cancellationToken);
    }
}
```

### Provider Discovery

```csharp
[EnhancedEnum("DataProviders", IncludeReferencedAssemblies = true)]
public abstract class DataProviderEnum : IEnhancedEnum<DataProviderEnum>
{
    public abstract string Name { get; }
    public abstract Type ParserType { get; }
    public abstract Type AdapterType { get; }
    public abstract ProviderCapabilities Capabilities { get; }
    
    public abstract IOperationParser CreateParser(IServiceProvider services);
    public abstract IConnectionAdapter CreateAdapter(ConnectionConfiguration config);
    public abstract Result<Unit> ValidateConfiguration(Dictionary<string, object> datum);
}
```

## 🔌 Creating Provider Packages

### Step 1: Create Provider Package

```bash
# Example: SQL Server provider
dotnet new classlib -n FractalDataWorks.Services.Connections.Data.SqlServer
dotnet add package FractalDataWorks.Services.Connections.Data
```

### Step 2: Implement Provider Enum

```csharp
using FractalDataWorks.Services.Connections.Data;

[EnumOption(Name = "SqlServer", Order = 1)]
public class SqlServerProvider : DataProviderEnum
{
    public override string Name => "SqlServer";
    public override Type ParserType => typeof(SqlServerOperationParser);
    public override Type AdapterType => typeof(SqlServerConnectionAdapter);
    
    public override ProviderCapabilities Capabilities => 
        ProviderCapabilities.BasicCrud | 
        ProviderCapabilities.Transactions | 
        ProviderCapabilities.StoredProcedures |
        ProviderCapabilities.ComplexQueries;
    
    public override IOperationParser CreateParser(IServiceProvider services) =>
        services.GetRequiredService<SqlServerOperationParser>();
        
    public override IConnectionAdapter CreateAdapter(ConnectionConfiguration config) =>
        new SqlServerConnectionAdapter(config);
        
    public override Result<Unit> ValidateConfiguration(Dictionary<string, object> datum)
    {
        if (!datum.TryGetValue("ConnectionString", out var connStr) || 
            string.IsNullOrEmpty(connStr?.ToString()))
            return Result<Unit>.Fail("ConnectionString is required for SQL Server");
            
        return ResultExtensions.Ok("Configuration valid");
    }
}
```

### Step 3: Implement Parser

```csharp
public class SqlServerOperationParser : IOperationParser
{
    public bool CanHandle(IDataOperation operation) => true;
    
    public async Task<Result<IParsedOperation>> Parse(
        IDataOperation operation, 
        DataContainerDefinition schema)
    {
        try
        {
            return operation.OperationType switch
            {
                OperationType.Query => ParseQuery((IDataQuery<object>)operation, schema),
                OperationType.Insert => ParseInsert((IDataInsert<object>)operation, schema),
                OperationType.Update => ParseUpdate((IDataUpdate<object>)operation, schema),
                OperationType.Delete => ParseDelete((IDataDelete<object>)operation, schema),
                _ => Result<IParsedOperation>.Fail($"Operation type {operation.OperationType} not supported")
            };
        }
        catch (Exception ex)
        {
            return Result<IParsedOperation>.Fail($"Failed to parse operation: {ex.Message}");
        }
    }
    
    private Result<IParsedOperation> ParseQuery(IDataQuery<object> query, DataContainerDefinition schema)
    {
        var sqlBuilder = new StringBuilder($"SELECT * FROM [{schema.SchemaName ?? "dbo"}].[{schema.Name}]");
        var parameters = new Dictionary<string, object>();
        
        if (query.WhereClause != null)
        {
            var whereTranslator = new SqlExpressionTranslator();
            var whereClause = whereTranslator.Translate(query.WhereClause, parameters);
            sqlBuilder.Append($" WHERE {whereClause}");
        }
        
        if (query.OrderBy != null)
        {
            var orderTranslator = new SqlExpressionTranslator();
            var orderClause = orderTranslator.TranslateOrderBy(query.OrderBy);
            sqlBuilder.Append($" ORDER BY {orderClause}");
            
            if (query.OrderByDescending)
                sqlBuilder.Append(" DESC");
        }
        
        if (query.Skip.HasValue || query.Take.HasValue)
        {
            sqlBuilder.Append($" OFFSET {query.Skip ?? 0} ROWS");
            if (query.Take.HasValue)
                sqlBuilder.Append($" FETCH NEXT {query.Take.Value} ROWS ONLY");
        }
        
        return Result<IParsedOperation>.Ok(new SqlParsedOperation
        {
            OriginalOperation = query,
            CommandText = sqlBuilder.ToString(),
            Parameters = parameters,
            Metadata = new OperationMetadata
            {
                OperationType = OperationType.Query,
                ContainerName = schema.Name
            }
        });
    }
}
```

### Step 4: Implement Adapter

```csharp
public class SqlServerConnectionAdapter : IConnectionAdapter
{
    private readonly string _connectionString;
    
    public SqlServerConnectionAdapter(ConnectionConfiguration config)
    {
        _connectionString = config.Datum["ConnectionString"].ToString()!;
    }
    
    public ProviderCapabilities Capabilities => 
        ProviderCapabilities.BasicCrud | 
        ProviderCapabilities.Transactions | 
        ProviderCapabilities.StoredProcedures;
    
    public async Task<Result<T>> Execute<T>(
        IParsedOperation operation, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            
            var sqlOperation = (SqlParsedOperation)operation;
            
            using var command = new SqlCommand(sqlOperation.CommandText, connection);
            
            foreach (var parameter in sqlOperation.Parameters)
            {
                command.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
            }
            
            return operation.OriginalOperation.OperationType switch
            {
                OperationType.Query => await ExecuteQuery<T>(command, cancellationToken),
                OperationType.Insert => await ExecuteInsert<T>(command, cancellationToken),
                OperationType.Update => await ExecuteUpdate<T>(command, cancellationToken),
                OperationType.Delete => await ExecuteDelete<T>(command, cancellationToken),
                _ => Result<T>.Fail($"Operation type not supported: {operation.OriginalOperation.OperationType}")
            };
        }
        catch (Exception ex)
        {
            return Result<T>.Fail($"Database operation failed: {ex.Message}");
        }
    }
    
    private async Task<Result<T>> ExecuteQuery<T>(SqlCommand command, CancellationToken cancellationToken)
    {
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var results = new List<object>();
        
        while (await reader.ReadAsync(cancellationToken))
        {
            var item = MapFromReader<T>(reader);
            results.Add(item);
        }
        
        return Result<T>.Ok((T)(object)results);
    }
}
```

### Step 5: Register Provider (Automatic!)

Thanks to Enhanced Enums, your provider is automatically discovered and registered when the assembly is loaded. No manual registration needed!

## 📋 Configuration

### appsettings.json

```json
{
  "Connections": {
    "default": {
      "ProviderName": "SqlServer",
      "Datum": {
        "ConnectionString": "Server=localhost;Database=MyApp;Integrated Security=true;",
        "CommandTimeout": 30
      }
    },
    "cache": {
      "ProviderName": "Redis", 
      "Datum": {
        "ConnectionString": "localhost:6379",
        "Database": 0
      }
    },
    "files": {
      "ProviderName": "JsonFile",
      "Datum": {
        "BasePath": "C:\\Data\\Files",
        "Format": "json"
      }
    }
  }
}
```

### Service Registration

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Automatic discovery and registration of all data providers
    services.AddFractalDataWorksConnections(Configuration);
    
    // Named connections
    services.AddNamedDataConnection("default", Configuration);
    services.AddNamedDataConnection("cache", Configuration);
    services.AddNamedDataConnection("files", Configuration);
}
```

## 🧪 Testing

### Unit Testing

```csharp
[Test]
public async Task DataConnection_Query_ShouldReturnResults()
{
    var mockAdapter = new Mock<IConnectionAdapter>();
    var mockParser = new Mock<IOperationParser>();
    
    var customers = new List<Customer>
    {
        new() { Id = "1", Name = "John", IsActive = true },
        new() { Id = "2", Name = "Jane", IsActive = true }
    };
    
    mockParser.Setup(p => p.Parse(It.IsAny<IDataOperation>(), It.IsAny<DataContainerDefinition>()))
           .ReturnsAsync(Result<IParsedOperation>.Ok(new MockParsedOperation()));
           
    mockAdapter.Setup(a => a.Execute<IEnumerable<Customer>>(It.IsAny<IParsedOperation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<Customer>>.Ok(customers));
    
    var connection = new DataConnection(mockParser.Object, mockAdapter.Object, new MockConfiguration());
    
    var result = await connection.Query<Customer>(c => c.IsActive);
    
    result.IsSuccess.ShouldBeTrue();
    result.Value.Count().ShouldBe(2);
}
```

### Integration Testing

```csharp
[Test]
public async Task SqlServerProvider_WithRealDatabase_ShouldWork()
{
    var configuration = new SqlServerDataConnectionConfiguration
    {
        ProviderName = "SqlServer",
        Datum = new Dictionary<string, object>
        {
            ["ConnectionString"] = "Server=(localdb)\\mssqllocaldb;Database=TestDb;Integrated Security=true;"
        }
    };
    
    var provider = DataProviders.ByName("SqlServer");
    var parser = provider.CreateParser(_serviceProvider);
    var adapter = provider.CreateAdapter(configuration);
    var connection = new DataConnection(parser, adapter, configuration);
    
    // Test actual database operations
    var customer = new Customer { Name = "Test Customer", Email = "test@example.com" };
    var insertResult = await connection.Insert(customer);
    
    insertResult.IsSuccess.ShouldBeTrue();
    customer.Id.ShouldNotBeNullOrEmpty();
    
    var queryResult = await connection.Single<Customer>(c => c.Id == customer.Id);
    queryResult.IsSuccess.ShouldBeTrue();
    queryResult.Value.Name.ShouldBe("Test Customer");
}
```

## 🔄 Version History

- **0.1.0-preview**: Initial release with universal data connection
- **Future**: Query optimization, connection pooling, caching, distributed transactions

## 📄 License

Licensed under the Apache License 2.0. See [LICENSE](../../LICENSE) for details.