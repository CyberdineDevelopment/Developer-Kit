# Enhanced Enum Integration Architecture Guide

## Overview

This guide describes how to use Enhanced Enums to create a plugin-based architecture for the FractalDataWorks SDK, enabling automatic service discovery, registration, and provider-based implementations.

## Core Concepts

### 1. Service Type Enhanced Enums

Service Types allow different implementations of the same interface to be discovered and registered automatically.

```csharp
// Base definition in FractalDataWorks.Services
[EnhancedEnum("ServiceTypes", IncludeReferencedAssemblies = true)]
public abstract class ServiceTypeEnum : IEnhancedEnum<ServiceTypeEnum>
{
    public abstract string Name { get; }
    public abstract Type InterfaceType { get; }
    public abstract Type ImplementationType { get; }
    public abstract string Provider { get; }
    public abstract int Priority { get; }
    
    // Factory method for creating instances
    public abstract IGenericService CreateInstance(IServiceProvider serviceProvider);
    
    // Configuration key for enabling/disabling
    public string ConfigKey => $"Services:{Provider}:{InterfaceType.Name}";
}
```

### 2. Connection Type Enhanced Enums

Connection Types enable different data providers to be plugged in without changing consuming code.

```csharp
// Base definition in FractalDataWorks.Connections.Data
[EnhancedEnum("ConnectionTypes", IncludeReferencedAssemblies = true)]
public abstract class ConnectionTypeEnum : IEnhancedEnum<ConnectionTypeEnum>
{
    public abstract string Name { get; }
    public abstract Type ConnectionType { get; }
    public abstract Type AdapterType { get; }
    public abstract Type ParserType { get; }
    
    // Factory for creating connection instances
    public abstract IDataConnection CreateConnection(ConnectionConfiguration config);
    
    // Validates configuration for this connection type
    public abstract Result<Unit> ValidateConfiguration(Dictionary<string, object> datum);
}
```

## Implementation Examples

### Service Type Implementations

#### Generic Project Management Service

```csharp
// In FractalDataWorks.Services.ProjectManagement
[EnumOption(Name = "GenericProjectManagement", Order = 100)]
public class GenericProjectManagementService : ServiceTypeEnum
{
    public override string Name => "GenericProjectManagement";
    public override Type InterfaceType => typeof(IProjectManagementService);
    public override Type ImplementationType => typeof(ProjectManagementService);
    public override string Provider => "Generic";
    public override int Priority => 100; // Lower priority, used as fallback
    
    public override IGenericService CreateInstance(IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<ProjectManagementService>>();
        var config = serviceProvider.GetRequiredService<ProjectManagementConfiguration>();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        
        return new ProjectManagementService(logger, config, mediator);
    }
}
```

#### Azure DevOps Project Management Service

```csharp
// In FractalDataWorks.Services.ProjectManagement.AzureDevOps (separate assembly)
[EnumOption(Name = "AzureDevOpsProjectManagement", Order = 10)]
public class AzureDevOpsProjectManagementService : ServiceTypeEnum
{
    public override string Name => "AzureDevOpsProjectManagement";
    public override Type InterfaceType => typeof(IProjectManagementService);
    public override Type ImplementationType => typeof(AzureDevOpsProjectManagementService);
    public override string Provider => "AzureDevOps";
    public override int Priority => 10; // Higher priority when enabled
    
    public override IGenericService CreateInstance(IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<AzureDevOpsProjectManagementService>>();
        var config = serviceProvider.GetRequiredService<AzureDevOpsConfiguration>();
        var httpClient = serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("AzureDevOps");
        
        return new AzureDevOpsProjectManagementService(logger, config, httpClient);
    }
}

// Custom handlers for Azure DevOps
public class AzureDevOpsCreateProjectHandler : IRequestHandler<CreateProjectCommand, Result<Project>>
{
    private readonly IHttpClient _httpClient;
    private readonly AzureDevOpsConfiguration _config;
    
    public async Task<Result<Project>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        // Create project via Azure DevOps REST API
        var azureProject = new
        {
            name = request.Name,
            description = request.Description,
            visibility = "private",
            capabilities = new
            {
                versioncontrol = new { sourceControlType = "Git" },
                processTemplate = new { templateTypeId = _config.ProcessTemplateId }
            }
        };
        
        var response = await _httpClient.PostAsJsonAsync(
            $"{_config.Organization}/_apis/projects?api-version=7.0",
            azureProject,
            cancellationToken);
            
        if (!response.IsSuccessStatusCode)
            return Result<Project>.Fail($"Azure DevOps error: {response.StatusCode}");
            
        var created = await response.Content.ReadFromJsonAsync<AzureDevOpsProject>();
        
        // Map to domain model
        var project = new Project
        {
            Id = created.Id,
            Name = created.Name,
            Description = created.Description,
            ExternalId = created.Id,
            ExternalSystem = "AzureDevOps"
        };
        
        return Result<Project>.Ok(project);
    }
}
```

### Connection Type Implementations

#### SQLite Connection

```csharp
// In FractalDataWorks.Connections.Sqlite
[EnumOption(Name = "SQLite", Order = 1)]
public class SqliteConnectionType : ConnectionTypeEnum
{
    public override string Name => "SQLite";
    public override Type ConnectionType => typeof(SqliteDataConnection);
    public override Type AdapterType => typeof(SqliteAdapter);
    public override Type ParserType => typeof(SqliteQueryParser);
    
    public override IDataConnection CreateConnection(ConnectionConfiguration config)
    {
        var parser = new SqliteQueryParser();
        var adapter = new SqliteAdapter(config.ConnectionString);
        return new DataConnection<SqliteAdapter, SqliteQueryParser>(adapter, parser, config);
    }
    
    public override Result<Unit> ValidateConfiguration(Dictionary<string, object> datum)
    {
        if (!datum.TryGetValue("ConnectionString", out var connStr) || string.IsNullOrEmpty(connStr?.ToString()))
            return Result<Unit>.Fail("ConnectionString is required for SQLite");
            
        return ResultExtensions.Success();
    }
}

public class SqliteQueryParser : IQueryParser
{
    public string ParseExpression<T>(Expression<Func<T, bool>> predicate)
    {
        var visitor = new SqliteExpressionVisitor();
        visitor.Visit(predicate);
        return visitor.GetSql();
    }
    
    public string ParseOrderBy<T>(Expression<Func<T, object>> orderBy, bool descending)
    {
        var visitor = new SqliteExpressionVisitor();
        visitor.Visit(orderBy);
        return $"{visitor.GetSql()} {(descending ? "DESC" : "ASC")}";
    }
}
```

#### REST API Connection

```csharp
// In FractalDataWorks.Connections.Rest
[EnumOption(Name = "RestApi", Order = 2)]
public class RestApiConnectionType : ConnectionTypeEnum
{
    public override string Name => "RestApi";
    public override Type ConnectionType => typeof(RestApiDataConnection);
    public override Type AdapterType => typeof(RestApiAdapter);
    public override Type ParserType => typeof(RestApiQueryParser);
    
    public override IDataConnection CreateConnection(ConnectionConfiguration config)
    {
        var parser = new RestApiQueryParser();
        var httpClient = new HttpClient { BaseAddress = new Uri(config.Datum["BaseUrl"].ToString()) };
        var adapter = new RestApiAdapter(httpClient, config);
        return new DataConnection<RestApiAdapter, RestApiQueryParser>(adapter, parser, config);
    }
    
    public override Result<Unit> ValidateConfiguration(Dictionary<string, object> datum)
    {
        if (!datum.TryGetValue("BaseUrl", out var baseUrl) || string.IsNullOrEmpty(baseUrl?.ToString()))
            return Result<Unit>.Fail("BaseUrl is required for REST API");
            
        if (!Uri.TryCreate(baseUrl.ToString(), UriKind.Absolute, out _))
            return Result<Unit>.Fail("Invalid BaseUrl format");
            
        return ResultExtensions.Success();
    }
}

public class RestApiQueryParser : IQueryParser
{
    public string ParseExpression<T>(Expression<Func<T, bool>> predicate)
    {
        // Convert LINQ expression to OData or custom query string format
        var visitor = new RestApiExpressionVisitor();
        visitor.Visit(predicate);
        return visitor.GetQueryString(); // e.g., "?$filter=Name eq 'Test'"
    }
}
```

## Generic Data Connection Implementation

```csharp
// Base data connection that uses parser and adapter
public class DataConnection<TAdapter, TParser> : IDataConnection
    where TAdapter : IDataConnectionAdapter
    where TParser : IQueryParser
{
    private readonly TAdapter _adapter;
    private readonly TParser _parser;
    private readonly ConnectionConfiguration _config;
    
    public DataConnection(TAdapter adapter, TParser parser, ConnectionConfiguration config)
    {
        _adapter = adapter;
        _parser = parser;
        _config = config;
    }
    
    public async Task<Result<IEnumerable<T>>> QueryAsync<T>(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
        where T : class
    {
        try
        {
            // Parser converts LINQ to provider-specific format
            var query = _parser.ParseExpression(predicate);
            
            // Adapter executes the query
            var results = await _adapter.ExecuteQueryAsync<T>(query, cancellationToken);
            
            return Result<IEnumerable<T>>.Ok(results);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<T>>.Fail($"Query failed: {ex.Message}");
        }
    }
    
    public async Task<Result<T>> SingleAsync<T>(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var result = await QueryAsync(predicate, cancellationToken);
        
        return result.Match(
            success: (items, msg) =>
            {
                var list = items.ToList();
                if (list.Count == 0)
                    return Result<T>.Fail("No items found");
                if (list.Count > 1)
                    return Result<T>.Fail("Multiple items found");
                return Result<T>.Ok(list[0]);
            },
            failure: error => Result<T>.Fail(error)
        );
    }
}
```

## Auto-Registration with Service Provider

```csharp
public interface IDataConnectionProvider
{
    IDataConnection GetConnection(string connectionName);
    IDataConnection GetConnection(ConnectionConfiguration config);
    Result<IDataConnection> TryGetConnection(string connectionName);
}

public class DataConnectionProvider : IDataConnectionProvider
{
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, Lazy<IDataConnection>> _connections;
    
    public DataConnectionProvider(IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _configuration = configuration;
        _serviceProvider = serviceProvider;
        _connections = new Dictionary<string, Lazy<IDataConnection>>();
        
        InitializeConnections();
    }
    
    private void InitializeConnections()
    {
        // Auto-discover all connection configurations
        var connectionConfigs = _configuration
            .GetSection("Connections")
            .Get<Dictionary<string, ConnectionConfiguration>>() ?? new();
            
        foreach (var (name, config) in connectionConfigs)
        {
            _connections[name] = new Lazy<IDataConnection>(() => CreateConnection(config));
        }
    }
    
    private IDataConnection CreateConnection(ConnectionConfiguration config)
    {
        // Find the appropriate connection type
        var connectionType = ConnectionTypes.All
            .FirstOrDefault(ct => ct.Name.Equals(config.Type, StringComparison.OrdinalIgnoreCase));
            
        if (connectionType == null)
            throw new InvalidOperationException($"Unknown connection type: {config.Type}");
            
        // Validate configuration
        var validationResult = connectionType.ValidateConfiguration(config.Datum);
        if (validationResult.IsFailure)
            throw new InvalidOperationException($"Invalid configuration: {validationResult}");
            
        return connectionType.CreateConnection(config);
    }
    
    public IDataConnection GetConnection(string connectionName)
    {
        if (!_connections.TryGetValue(connectionName, out var lazyConnection))
            throw new KeyNotFoundException($"Connection '{connectionName}' not found");
            
        return lazyConnection.Value;
    }
}
```

## Service Registration Extensions

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFractalDataWorksServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register base services
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
            AppDomain.CurrentDomain.GetAssemblies()));
        
        // Auto-register all service types
        foreach (var serviceType in ServiceTypes.All)
        {
            var configKey = serviceType.ConfigKey;
            var isEnabled = configuration.GetValue<bool>(configKey, true);
            
            if (isEnabled)
            {
                services.AddScoped(serviceType.InterfaceType, provider => 
                    serviceType.CreateInstance(provider));
            }
        }
        
        // Register data connection provider
        services.AddSingleton<IDataConnectionProvider, DataConnectionProvider>();
        
        // Register individual connections as named services
        services.AddScoped<Func<string, IDataConnection>>(provider =>
        {
            var connectionProvider = provider.GetRequiredService<IDataConnectionProvider>();
            return name => connectionProvider.GetConnection(name);
        });
        
        return services;
    }
    
    public static IServiceCollection AddServiceProvider<TProvider>(
        this IServiceCollection services,
        string providerName)
        where TProvider : ServiceTypeEnum
    {
        // Register only services from a specific provider
        var providerServices = ServiceTypes.All
            .Where(st => st.Provider == providerName);
            
        foreach (var serviceType in providerServices)
        {
            services.AddScoped(serviceType.InterfaceType, provider => 
                serviceType.CreateInstance(provider));
        }
        
        return services;
    }
}
```

## Usage in Handlers

```csharp
public class GetProjectQueryHandler : IRequestHandler<GetProjectQuery, Result<Project>>
{
    private readonly IDataConnectionProvider _connectionProvider;
    private readonly IConfiguration _configuration;
    
    public GetProjectQueryHandler(
        IDataConnectionProvider connectionProvider,
        IConfiguration configuration)
    {
        _connectionProvider = connectionProvider;
        _configuration = configuration;
    }
    
    public async Task<Result<Project>> Handle(
        GetProjectQuery request, 
        CancellationToken cancellationToken)
    {
        // Get connection based on configuration
        var connectionName = _configuration["ProjectDataConnection"] ?? "default";
        var connection = _connectionProvider.GetConnection(connectionName);
        
        // Use LINQ regardless of underlying provider
        var result = await connection.SingleAsync<Project>(
            p => p.Id == request.ProjectId,
            cancellationToken);
            
        if (result.IsFailure)
            return result;
            
        // Include related data if needed
        if (request.IncludeEpics)
        {
            var epicsResult = await connection.QueryAsync<Epic>(
                e => e.ProjectId == request.ProjectId,
                cancellationToken);
                
            if (epicsResult.IsSuccess)
            {
                result.Value.Epics = epicsResult.Value.ToList();
            }
        }
        
        return result;
    }
}
```

## Configuration Example

```json
{
  "Services": {
    "AzureDevOps": {
      "IProjectManagementService": true
    },
    "Generic": {
      "IProjectManagementService": false
    }
  },
  "Connections": {
    "default": {
      "Type": "SQLite",
      "Datum": {
        "ConnectionString": "Data Source=app.db"
      }
    },
    "reporting": {
      "Type": "SqlServer",
      "Datum": {
        "ConnectionString": "Server=...;Database=Reporting;",
        "CommandTimeout": 30
      }
    },
    "external": {
      "Type": "RestApi",
      "Datum": {
        "BaseUrl": "https://api.example.com",
        "ApiKey": "secret",
        "Timeout": 30
      }
    }
  },
  "ProjectDataConnection": "default"
}
```

## Benefits of This Architecture

### 1. Plugin-Based Extensibility
- New service implementations can be added without modifying core code
- New data providers can be added by implementing the connection interfaces
- Cross-assembly scanning automatically discovers new implementations

### 2. Configuration-Driven Behavior
- Switch between providers through configuration
- Enable/disable specific implementations without code changes
- Different environments can use different providers

### 3. Consistent Programming Model
- Use LINQ for all data access regardless of provider
- Same Result<T> pattern throughout
- Service implementations are thin orchestrators

### 4. Type Safety
- Enhanced Enums provide compile-time safety
- No magic strings for provider names
- Strong typing throughout the stack

### 5. Testability
- Mock IDataConnectionProvider for unit tests
- Use in-memory SQLite for integration tests
- Test parsers and adapters independently

## Advanced Patterns

### Custom Query Hints

```csharp
[EnhancedEnum("QueryHints")]
public abstract class QueryHintEnum : IEnhancedEnum
{
    public abstract string Name { get; }
    public abstract string ApplyToQuery(string query, ConnectionTypeEnum connectionType);
}

[EnumOption(Name = "NoLock")]
public class NoLockHint : QueryHintEnum
{
    public override string Name => "NoLock";
    
    public override string ApplyToQuery(string query, ConnectionTypeEnum connectionType)
    {
        return connectionType.Name switch
        {
            "SqlServer" => query.Replace("FROM", "FROM WITH (NOLOCK)"),
            _ => query // Not applicable to other providers
        };
    }
}
```

### Provider Capabilities

```csharp
[Flags]
public enum ConnectionCapabilities
{
    None = 0,
    Transactions = 1,
    BulkInsert = 2,
    StoredProcedures = 4,
    FullTextSearch = 8,
    JsonColumns = 16
}

public abstract class ConnectionTypeEnum : IEnhancedEnum<ConnectionTypeEnum>
{
    public abstract ConnectionCapabilities Capabilities { get; }
    
    public bool Supports(ConnectionCapabilities capability) => 
        (Capabilities & capability) == capability;
}
```

## Summary

This architecture leverages Enhanced Enums to create a highly extensible, plugin-based system where:

1. **Service implementations** can be swapped based on configuration
2. **Data providers** can be added without changing consuming code
3. **Business logic** remains consistent across providers
4. **Cross-assembly scanning** automatically discovers new implementations
5. **Type safety** is maintained throughout

The result is a system that's both flexible and maintainable, allowing teams to add new providers and implementations without modifying core framework code.
