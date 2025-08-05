# FractalDataWorks.Services.ExternalConnections.Abstractions

The **FractalDataWorks.Services.ExternalConnections.Abstractions** package provides the foundation for connecting to external systems in the FractalDataWorks Framework. This package defines interfaces and base classes for database connections, API connections, file systems, message queues, and other external resources.

## Overview

This abstraction layer provides:

- **Unified Connection Interface** - Common pattern for all external connections
- **Provider Discovery** - Automatic discovery and registration of connection providers via EnhancedEnums
- **Connection Factories** - Factory pattern for creating and managing connections
- **Configuration Management** - Type-safe configuration for different connection types
- **Connection Lifecycle** - Open, close, test, and health check operations
- **Provider Metadata** - Rich metadata about connection capabilities and features

## Quick Start

### Using an Existing Connection Provider

```csharp
using FractalDataWorks.Services.ExternalConnections.Abstractions;
using FractalDataWorks.Framework.Abstractions;

// Configuration for SQL Server connection
public sealed class SqlServerConfiguration : FdwConfigurationBase
{
    public string ConnectionString { get; set; } = string.Empty;
    public int CommandTimeout { get; set; } = 30;
    public bool EnableConnectionPooling { get; set; } = true;
    
    public override IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(ConnectionString))
            errors.Add("Connection string is required");
            
        if (CommandTimeout <= 0)
            errors.Add("Command timeout must be positive");
            
        return errors;
    }
}

// Using the connection
public async Task<IFdwResult> UseDatabaseConnectionAsync()
{
    // Find the SQL Server connection provider
    var provider = ExternalConnectionProviders.GetByDataStore("SqlServer").FirstOrDefault();
    if (provider == null)
        return FdwResult.Failure("SQL Server provider not found");
    
    // Create connection factory
    var factoryResult = await provider.CreateConnectionFactoryAsync(_serviceProvider);
    if (factoryResult.IsFailure)
        return FdwResult.Failure($"Failed to create factory: {factoryResult.ErrorMessage}");
    
    var factory = factoryResult.Value;
    
    // Create connection
    var config = new SqlServerConfiguration
    {
        ConnectionString = "Server=localhost;Database=MyApp;Trusted_Connection=true;",
        CommandTimeout = 60
    };
    
    var connectionResult = await factory.CreateConnectionAsync(config);
    if (connectionResult.IsFailure)
        return FdwResult.Failure($"Failed to create connection: {connectionResult.ErrorMessage}");
    
    var connection = connectionResult.Value;
    
    try
    {
        // Open and use the connection  
        var openResult = await connection.OpenAsync();
        if (openResult.IsFailure)
            return FdwResult.Failure($"Failed to open connection: {openResult.ErrorMessage}");
        
        // Use connection for database operations
        Console.WriteLine($"Connected to {connection.ProviderName}");
        Console.WriteLine($"Connection State: {connection.State}");
        
        // Get metadata about the connected system
        var metadataResult = await connection.GetMetadataAsync();
        if (metadataResult.IsSuccess)
        {
            var metadata = metadataResult.Value;
            Console.WriteLine($"Server Version: {metadata.Version}");
        }
        
        return FdwResult.Success();
    }
    finally
    {
        await connection.CloseAsync();
        connection.Dispose();
    }
}
```

### Creating a Custom Connection Provider

```csharp
// Define configuration for your custom connection
public sealed class MongoDbConfiguration : FdwConfigurationBase
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public int MaxConnectionPoolSize { get; set; } = 100;
    public bool UseTls { get; set; } = true;
    
    public override IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(ConnectionString))
            errors.Add("MongoDB connection string is required");
            
        if (string.IsNullOrWhiteSpace(DatabaseName))
            errors.Add("Database name is required");
            
        if (MaxConnectionPoolSize <= 0)
            errors.Add("Max connection pool size must be positive");
            
        return errors;
    }
}

// Implement the connection
public sealed class MongoDbConnection : IExternalConnection<MongoDbConfiguration>
{
    private MongoClient? _client;
    private IMongoDatabase? _database;
    private MongoDbConfiguration? _configuration;
    private bool _disposed;
    
    public string ConnectionId { get; } = Guid.NewGuid().ToString();
    public string ProviderName => "MongoDB";
    public FdwConnectionState State { get; private set; } = FdwConnectionState.Closed;
    public string ConnectionString => SanitizeConnectionString(_configuration?.ConnectionString ?? "");
    public MongoDbConfiguration Configuration => _configuration ?? throw new InvalidOperationException("Connection not initialized");
    
    public async Task<IFdwResult> InitializeAsync(MongoDbConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        
        if (_configuration != null)
            return FdwResult.Failure("Connection already initialized");
        
        try
        {
            var settings = MongoClientSettings.FromConnectionString(configuration.ConnectionString);
            settings.MaxConnectionPoolSize = configuration.MaxConnectionPoolSize;
            settings.UseTls = configuration.UseTls;
            
            _client = new MongoClient(settings);
            _configuration = configuration;
            
            return FdwResult.Success();
        }
        catch (Exception ex)
        {
            return FdwResult.Failure("Failed to initialize MongoDB connection", ex);
        }
    }
    
    public async Task<IFdwResult> OpenAsync()
    {
        if (_client == null || _configuration == null)
            return FdwResult.Failure("Connection not initialized");
            
        if (State == FdwConnectionState.Open)
            return FdwResult.Failure("Connection is already open");
            
        try
        {
            _database = _client.GetDatabase(_configuration.DatabaseName);
            
            // Test the connection by running a simple command
            await _database.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1));
            
            State = FdwConnectionState.Open;
            return FdwResult.Success();
        }
        catch (Exception ex)
        {
            State = FdwConnectionState.Broken;
            return FdwResult.Failure("Failed to open MongoDB connection", ex);
        }
    }
    
    public async Task<IFdwResult> CloseAsync()
    {
        if (State == FdwConnectionState.Closed)
            return FdwResult.Success(); // Already closed
            
        try
        {
            _database = null;
            // MongoDB client doesn't have an explicit close method
            State = FdwConnectionState.Closed;
            return FdwResult.Success();
        }
        catch (Exception ex)
        {
            return FdwResult.Failure("Failed to close MongoDB connection", ex);
        }
    }
    
    public async Task<IFdwResult> TestConnectionAsync()
    {
        if (_client == null || _configuration == null)
            return FdwResult.Failure("Connection not initialized");
            
        try
        {
            var tempDb = _client.GetDatabase(_configuration.DatabaseName);
            await tempDb.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1));
            return FdwResult.Success();
        }
        catch (Exception ex)
        {
            return FdwResult.Failure("MongoDB connection test failed", ex);
        }
    }
    
    public async Task<IFdwResult<IConnectionMetadata>> GetMetadataAsync()
    {
        if (_database == null)
            return FdwResult<IConnectionMetadata>.Failure("Connection not open");
            
        try
        {
            var serverStatus = await _database.RunCommandAsync<BsonDocument>(new BsonDocument("serverStatus", 1));
            var metadata = new MongoDbMetadata
            {
                Version = serverStatus["version"].AsString,
                DatabaseName = _database.DatabaseNamespace.DatabaseName,
                MaxConnectionPoolSize = _configuration?.MaxConnectionPoolSize ?? 0
            };
            
            return FdwResult<IConnectionMetadata>.Success(metadata);
        }
        catch (Exception ex)
        {
            return FdwResult<IConnectionMetadata>.Failure("Failed to get MongoDB metadata", ex);
        }
    }
    
    private string SanitizeConnectionString(string connectionString)
    {
        // Remove sensitive information for logging
        return Regex.Replace(connectionString, @"password=[^;]*", "password=***", RegexOptions.IgnoreCase);
    }
    
    public void Dispose()
    {
        if (!_disposed)
        {
            CloseAsync().GetAwaiter().GetResult();
            _client?.Dispose();
            _disposed = true;
        }
    }
}

// Implement connection factory
public sealed class MongoDbConnectionFactory : IExternalConnectionFactory<MongoDbConfiguration, MongoDbConnection>
{
    public string ProviderName => "MongoDB";
    public IReadOnlyList<string> SupportedConnectionTypes => new[] { "ReadWrite", "ReadOnly" };
    public Type ConfigurationType => typeof(MongoDbConfiguration);
    
    public async Task<IFdwResult<MongoDbConnection>> CreateConnectionAsync(MongoDbConfiguration configuration)
    {
        try
        {
            var connection = new MongoDbConnection();
            var initResult = await connection.InitializeAsync(configuration);
            
            return initResult.IsSuccess 
                ? FdwResult<MongoDbConnection>.Success(connection)
                : FdwResult<MongoDbConnection>.Failure(initResult.ErrorMessage, initResult.Exception);
        }
        catch (Exception ex)
        {
            return FdwResult<MongoDbConnection>.Failure("Failed to create MongoDB connection", ex);
        }
    }
    
    public async Task<IFdwResult<MongoDbConnection>> CreateConnectionAsync(MongoDbConfiguration configuration, string connectionType)
    {
        if (!SupportedConnectionTypes.Contains(connectionType))
            return FdwResult<MongoDbConnection>.Failure($"Connection type '{connectionType}' is not supported");
            
        // For this example, both connection types create the same connection
        // In practice, you might create different connection instances
        return await CreateConnectionAsync(configuration);
    }
    
    public async Task<IFdwResult> ValidateConfigurationAsync(MongoDbConfiguration configuration)
    {
        var validationErrors = configuration.Validate();
        return validationErrors.Count == 0 
            ? FdwResult.Success()
            : FdwResult.Failure("Configuration validation failed", validationErrors);
    }
    
    public async Task<IFdwResult> TestConnectivityAsync(MongoDbConfiguration configuration)
    {
        try
        {
            using var connection = new MongoDbConnection();
            var initResult = await connection.InitializeAsync(configuration);
            if (initResult.IsFailure)
                return initResult;
                
            return await connection.TestConnectionAsync();
        }
        catch (Exception ex)
        {
            return FdwResult.Failure("Connectivity test failed", ex);
        }
    }
    
    // Non-generic implementations (required by base interface)
    public async Task<IFdwResult<IExternalConnection>> CreateConnectionAsync(FdwConfigurationBase configuration)
    {
        if (configuration is not MongoDbConfiguration mongoConfig)
            return FdwResult<IExternalConnection>.Failure("Invalid configuration type");
            
        var result = await CreateConnectionAsync(mongoConfig);
        return result.IsSuccess 
            ? FdwResult<IExternalConnection>.Success(result.Value)
            : FdwResult<IExternalConnection>.Failure(result.ErrorMessage, result.Exception);
    }
    
    public async Task<IFdwResult<IExternalConnection>> CreateConnectionAsync(FdwConfigurationBase configuration, string connectionType)
    {
        if (configuration is not MongoDbConfiguration mongoConfig)
            return FdwResult<IExternalConnection>.Failure("Invalid configuration type");
            
        var result = await CreateConnectionAsync(mongoConfig, connectionType);
        return result.IsSuccess 
            ? FdwResult<IExternalConnection>.Success(result.Value)
            : FdwResult<IExternalConnection>.Failure(result.ErrorMessage, result.Exception);
    }
    
    public async Task<IFdwResult> ValidateConfigurationAsync(FdwConfigurationBase configuration)
    {
        if (configuration is not MongoDbConfiguration mongoConfig)
            return FdwResult.Failure("Invalid configuration type");
            
        return await ValidateConfigurationAsync(mongoConfig);
    }
    
    public async Task<IFdwResult> TestConnectivityAsync(FdwConfigurationBase configuration)
    {
        if (configuration is not MongoDbConfiguration mongoConfig)
            return FdwResult.Failure("Invalid configuration type");
            
        return await TestConnectivityAsync(mongoConfig);
    }
}

// Create the connection provider for auto-discovery
public sealed class MongoDbConnectionProvider : ExternalConnectionProviderBase
{
    public static readonly MongoDbConnectionProvider Instance = new();
    
    private MongoDbConnectionProvider() : base(
        id: 1,
        name: "MongoDB Connection Provider",
        supportedDataStores: new[] { "MongoDB", "DocumentDb" },
        providerName: "MongoDB.Driver",
        connectionType: typeof(MongoDbConnection),
        configurationType: typeof(MongoDbConfiguration),
        supportedConnectionModes: new[] { "ReadWrite", "ReadOnly" },
        priority: 100)
    {
    }
    
    public override async Task<IFdwResult<IExternalConnectionFactory>> CreateConnectionFactoryAsync(IServiceProvider serviceProvider)
    {
        try
        {
            var factory = new MongoDbConnectionFactory();
            return FdwResult<IExternalConnectionFactory>.Success(factory);
        }
        catch (Exception ex)
        {
            return FdwResult<IExternalConnectionFactory>.Failure("Failed to create MongoDB connection factory", ex);
        }
    }
    
    public override void RegisterService(IServiceCollection services)
    {
        services.AddSingleton<MongoDbConnectionFactory>();
        services.AddTransient<MongoDbConnection>();
    }
    
    public override async Task<IFdwResult<IProviderMetadata>> GetProviderMetadataAsync()
    {
        var metadata = new MongoDbProviderMetadata
        {
            ProviderName = ProviderName,
            Version = "2.23.1", // MongoDB.Driver version
            SupportedDataStores = SupportedDataStores,
            SupportedConnectionModes = SupportedConnectionModes,
            Features = new[] { "Transactions", "Aggregation", "GridFS", "ChangeStreams" },
            MaxConnections = 10000,
            SupportsAsync = true,
            SupportsTransactions = true
        };
        
        return FdwResult<IProviderMetadata>.Success(metadata);
    }
}
```

## Implementation Examples

### SQL Server Connection Provider

```csharp
public sealed class SqlServerConfiguration : FdwConfigurationBase
{
    public string ConnectionString { get; set; } = string.Empty;
    public int CommandTimeout { get; set; } = 30;
    public bool EnableRetryLogic { get; set; } = true;
    public int MaxRetryCount { get; set; } = 3;
    
    public override IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(ConnectionString))
            errors.Add("Connection string is required");
            
        if (CommandTimeout <= 0)
            errors.Add("Command timeout must be positive");
            
        if (MaxRetryCount < 0)
            errors.Add("Max retry count cannot be negative");
            
        return errors;
    }
}

public sealed class SqlServerConnectionProvider : ExternalConnectionProviderBase
{
    public static readonly SqlServerConnectionProvider Instance = new();
    
    private SqlServerConnectionProvider() : base(
        id: 2,
        name: "SQL Server Connection Provider",
        supportedDataStores: new[] { "SqlServer", "AzureSql" },
        providerName: "Microsoft.Data.SqlClient",
        connectionType: typeof(SqlServerConnection),
        configurationType: typeof(SqlServerConfiguration),
        supportedConnectionModes: new[] { "ReadWrite", "ReadOnly", "Bulk" },
        priority: 200)
    {
    }
    
    // Implementation methods...
}
```

### REST API Connection Provider

```csharp
public sealed class RestApiConfiguration : FdwConfigurationBase
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
    public Dictionary<string, string> DefaultHeaders { get; set; } = new();
    
    public override IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(BaseUrl))
            errors.Add("Base URL is required");
            
        if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out _))
            errors.Add("Base URL must be a valid absolute URI");
            
        if (TimeoutSeconds <= 0)
            errors.Add("Timeout must be positive");
            
        return errors;
    }
}

public sealed class RestApiConnection : IExternalConnection<RestApiConfiguration>
{
    private HttpClient? _httpClient;
    private RestApiConfiguration? _configuration;
    
    public string ConnectionId { get; } = Guid.NewGuid().ToString();
    public string ProviderName => "REST API";
    public FdwConnectionState State { get; private set; } = FdwConnectionState.Closed;
    public string ConnectionString => _configuration?.BaseUrl ?? "";
    public RestApiConfiguration Configuration => _configuration ?? throw new InvalidOperationException("Connection not initialized");
    
    public async Task<IFdwResult> InitializeAsync(RestApiConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        
        try
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(configuration.BaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(configuration.TimeoutSeconds);
            
            // Set default headers
            foreach (var header in configuration.DefaultHeaders)
            {
                _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
            
            // Set API key if provided
            if (!string.IsNullOrWhiteSpace(configuration.ApiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {configuration.ApiKey}");
            }
            
            _configuration = configuration;
            return FdwResult.Success();
        }
        catch (Exception ex)
        {
            return FdwResult.Failure("Failed to initialize REST API connection", ex);
        }
    }
    
    public async Task<IFdwResult> OpenAsync()
    {
        if (_httpClient == null)
            return FdwResult.Failure("Connection not initialized");
            
        try
        {
            // Test connectivity by making a HEAD request to base URL
            var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, ""));
            State = response.IsSuccessStatusCode ? FdwConnectionState.Open : FdwConnectionState.Broken;
            
            return response.IsSuccessStatusCode 
                ? FdwResult.Success()
                : FdwResult.Failure($"API responded with status: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            State = FdwConnectionState.Broken;
            return FdwResult.Failure("Failed to connect to API", ex);
        }
    }
    
    public async Task<IFdwResult> TestConnectionAsync()
    {
        if (_httpClient == null)
            return FdwResult.Failure("Connection not initialized");
            
        try
        {
            var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, ""));
            return response.IsSuccessStatusCode 
                ? FdwResult.Success()
                : FdwResult.Failure($"API test failed with status: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return FdwResult.Failure("API connection test failed", ex);
        }
    }
    
    // Additional methods for REST operations
    public async Task<IFdwResult<T>> GetAsync<T>(string endpoint)
    {
        if (State != FdwConnectionState.Open)
            return FdwResult<T>.Failure("Connection is not open");
            
        try
        {
            var response = await _httpClient!.GetAsync(endpoint);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<T>(json);
                return FdwResult<T>.Success(result);
            }
            
            return FdwResult<T>.Failure($"API request failed: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return FdwResult<T>.Failure("API request failed", ex);
        }
    }
    
    // Implement other interface members...
}

public sealed class RestApiConnectionProvider : ExternalConnectionProviderBase
{
    public static readonly RestApiConnectionProvider Instance = new();
    
    private RestApiConnectionProvider() : base(
        id: 3,
        name: "REST API Connection Provider",
        supportedDataStores: new[] { "RestApi", "WebApi", "HttpApi" },
        providerName: "System.Net.Http.HttpClient",
        connectionType: typeof(RestApiConnection),
        configurationType: typeof(RestApiConfiguration),
        supportedConnectionModes: new[] { "ReadWrite", "ReadOnly" },
        priority: 150)
    {
    }
    
    // Implementation methods...
}
```

### Oracle Database Connection Provider

```csharp
public sealed class OracleConnectionProvider : ExternalConnectionProviderBase
{
    public static readonly OracleConnectionProvider Instance = new();
    
    private OracleConnectionProvider() : base(
        id: 4,
        name: "Oracle Database Connection Provider",
        supportedDataStores: new[] { "Oracle" },
        providerName: "Oracle.ManagedDataAccess.Client",
        connectionType: typeof(OracleConnection),
        configurationType: typeof(OracleConfiguration),
        supportedConnectionModes: new[] { "ReadWrite", "ReadOnly", "Bulk" },
        priority: 180)
    {
    }
    
    // Implementation...
}
```

## Configuration Examples

### JSON Configuration for Multiple Providers

```json
{
  "ExternalConnections": {
    "PrimaryDatabase": {
      "Provider": "SqlServer",
      "ConnectionString": "Server=prod-sql;Database=MyApp;Trusted_Connection=true;",
      "CommandTimeout": 60,
      "EnableRetryLogic": true,
      "MaxRetryCount": 3
    },
    "DocumentStore": {
      "Provider": "MongoDB",
      "ConnectionString": "mongodb://localhost:27017",
      "DatabaseName": "MyAppDocs",
      "MaxConnectionPoolSize": 50,
      "UseTls": false
    },
    "PaymentApi": {
      "Provider": "RestApi",
      "BaseUrl": "https://api.payments.com/v1",
      "ApiKey": "pk_live_...",
      "TimeoutSeconds": 45,
      "DefaultHeaders": {
        "Accept": "application/json",
        "User-Agent": "MyApp/1.0"
      }
    },
    "ReportingDatabase": {
      "Provider": "Oracle",
      "ConnectionString": "Data Source=oracle-server:1521/XE;User Id=reports;Password=***;",
      "CommandTimeout": 120
    }
  }
}
```

### Dependency Injection Registration

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Configure connection settings
    services.Configure<SqlServerConfiguration>(
        Configuration.GetSection("ExternalConnections:PrimaryDatabase"));
    services.Configure<MongoDbConfiguration>(
        Configuration.GetSection("ExternalConnections:DocumentStore"));
    services.Configure<RestApiConfiguration>(
        Configuration.GetSection("ExternalConnections:PaymentApi"));
    
    // Register connection providers (auto-discovered via ExternalConnectionProviders)
    services.AddExternalConnectionProviders();
    
    // Register application services that use connections
    services.AddScoped<IUserService, UserService>();
    services.AddScoped<IPaymentService, PaymentService>();
}

// Extension method for bulk registration
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddExternalConnectionProviders(this IServiceCollection services)
    {
        // Register all discovered connection providers
        foreach (var provider in ExternalConnectionProviders.All)
        {
            provider.RegisterService(services);
        }
        
        return services;
    }
}
```

## Advanced Usage

### Connection Pool Management

```csharp
public sealed class ConnectionPoolManager : IDisposable
{
    private readonly Dictionary<string, ObjectPool<IExternalConnection>> _pools = new();
    private readonly IServiceProvider _serviceProvider;
    
    public ConnectionPoolManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public async Task<IFdwResult<IExternalConnection>> GetConnectionAsync(string providerName, FdwConfigurationBase configuration)
    {
        if (!_pools.TryGetValue(providerName, out var pool))
        {
            // Create pool for this provider
            var provider = ExternalConnectionProviders.GetByProvider(providerName).FirstOrDefault();
            if (provider == null)
                return FdwResult<IExternalConnection>.Failure($"Provider '{providerName}' not found");
                
            pool = CreateConnectionPool(provider, configuration);
            _pools[providerName] = pool;
        }
        
        var connection = pool.Get();
        if (connection.State != FdwConnectionState.Open)
        {
            var openResult = await connection.OpenAsync();
            if (openResult.IsFailure)
            {
                pool.Return(connection);
                return FdwResult<IExternalConnection>.Failure(openResult.ErrorMessage);
            }
        }
        
        return FdwResult<IExternalConnection>.Success(connection);
    }
    
    public void ReturnConnection(string providerName, IExternalConnection connection)
    {
        if (_pools.TryGetValue(providerName, out var pool))
        {
            pool.Return(connection);
        }
    }
    
    private ObjectPool<IExternalConnection> CreateConnectionPool(IExternalConnectionProvider provider, FdwConfigurationBase configuration)
    {
        // Implementation of connection pool creation
        // This is a simplified example
        return new DefaultObjectPool<IExternalConnection>(
            new ConnectionPooledObjectPolicy(provider, configuration));
    }
    
    public void Dispose()
    {
        foreach (var pool in _pools.Values)
        {
            pool.Dispose();
        }
        _pools.Clear();
    }
}
```

### Health Monitoring

```csharp
public sealed class ConnectionHealthMonitor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ConnectionHealthMonitor> _logger;
    
    public ConnectionHealthMonitor(IServiceProvider serviceProvider, ILogger<ConnectionHealthMonitor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckAllProvidersHealth();
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
    
    private async Task CheckAllProvidersHealth()
    {
        foreach (var provider in ExternalConnectionProviders.All)
        {
            try
            {
                var metadataResult = await provider.GetProviderMetadataAsync();
                if (metadataResult.IsFailure)
                {
                    _logger.LogWarning("Provider {Provider} health check failed: {Error}", 
                        provider.ProviderName, metadataResult.ErrorMessage);
                }
                else
                {
                    _logger.LogDebug("Provider {Provider} is healthy", provider.ProviderName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during health check for provider {Provider}", provider.ProviderName);
            }
        }
    }
}
```

### Multi-tenant Connection Management

```csharp
public sealed class TenantConnectionManager
{
    private readonly Dictionary<string, Dictionary<string, IExternalConnectionFactory>> _tenantConnections = new();
    private readonly IServiceProvider _serviceProvider;
    
    public TenantConnectionManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public async Task<IFdwResult<IExternalConnection>> GetTenantConnectionAsync(string tenantId, string dataStore)
    {
        if (!_tenantConnections.TryGetValue(tenantId, out var tenantFactories))
        {
            return FdwResult<IExternalConnection>.Failure($"No connections configured for tenant {tenantId}");
        }
        
        if (!tenantFactories.TryGetValue(dataStore, out var factory))
        {
            return FdwResult<IExternalConnection>.Failure($"No {dataStore} connection for tenant {tenantId}");
        }
        
        // Get tenant-specific configuration
        var config = await GetTenantConfigurationAsync(tenantId, dataStore);
        if (config == null)
        {
            return FdwResult<IExternalConnection>.Failure($"No configuration found for tenant {tenantId}, dataStore {dataStore}");
        }
        
        return await factory.CreateConnectionAsync(config);
    }
    
    public async Task InitializeTenantAsync(string tenantId, Dictionary<string, FdwConfigurationBase> configurations)
    {
        var tenantFactories = new Dictionary<string, IExternalConnectionFactory>();
        
        foreach (var (dataStore, config) in configurations)
        {
            var provider = ExternalConnectionProviders.GetByDataStore(dataStore).FirstOrDefault();
            if (provider != null)
            {
                var factoryResult = await provider.CreateConnectionFactoryAsync(_serviceProvider);
                if (factoryResult.IsSuccess)
                {
                    tenantFactories[dataStore] = factoryResult.Value;
                }
            }
        }
        
        _tenantConnections[tenantId] = tenantFactories;
    }
    
    private async Task<FdwConfigurationBase?> GetTenantConfigurationAsync(string tenantId, string dataStore)
    {
        // Load tenant-specific configuration from your configuration store
        // This is a simplified example
        return new SqlServerConfiguration
        {
            ConnectionString = $"Server=tenant-{tenantId}-db;Database=App;Trusted_Connection=true;"
        };
    }
}
```

## Integration with Data Providers

External connections are commonly used by data providers:

```csharp
public sealed class SqlDataProvider : IDataProvider
{
    private readonly IExternalConnection _connection;
    
    public SqlDataProvider(IExternalConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }
    
    public async Task<IFdwResult<object?>> Execute(IDataCommand command)
    {
        if (_connection.State != FdwConnectionState.Open)
        {
            var openResult = await _connection.OpenAsync();
            if (openResult.IsFailure)
                return FdwResult<object?>.Failure("Failed to open connection", openResult.Exception);
        }
        
        // Use the connection to execute the command
        // Implementation depends on the specific connection type
        return FdwResult<object?>.Success(null);
    }
}
```

## Best Practices

1. **Always dispose connections** to prevent resource leaks
2. **Use connection pooling** for high-throughput scenarios
3. **Implement proper retry logic** for transient failures
4. **Sanitize connection strings** when logging
5. **Use typed configurations** for compile-time safety
6. **Test connectivity** during application startup
7. **Monitor connection health** proactively
8. **Handle timeouts gracefully** in your applications
9. **Use appropriate connection modes** (ReadOnly vs ReadWrite)
10. **Cache provider metadata** to avoid repeated lookups

## License

This package is part of the FractalDataWorks Framework and is licensed under the Apache 2.0 License.