# FractalDataWorks.Services.DataProviders.Abstractions

The **FractalDataWorks.Services.DataProviders.Abstractions** package provides the foundation for executing data commands across different storage technologies in the FractalDataWorks Framework. This package defines interfaces and base classes for SQL databases, NoSQL stores, APIs, file systems, and other data sources.

## Overview

This abstraction layer provides:

- **Unified Command Interface** - Consistent data operations across different storage technologies
- **Provider Discovery** - Automatic discovery and registration of data providers via EnhancedEnums
- **Transaction Support** - Atomic operations with commit/rollback capabilities
- **Batch Processing** - Efficient execution of multiple commands
- **Type Safety** - Strongly-typed command execution with generic interfaces
- **Performance Metrics** - Built-in monitoring and performance tracking

## Quick Start

### Using an Existing Data Provider

```csharp
using FractalDataWorks.Services.DataProviders.Abstractions;
using FractalDataWorks.Framework.Abstractions;

// Define a simple query command
public sealed class UserQueryCommand : IDataCommand<List<User>>
{
    public string CommandId { get; } = Guid.NewGuid().ToString();
    public string CommandType => "Query";
    public string? Target => "Users";
    public Type ExpectedResultType => typeof(List<User>);
    public TimeSpan? Timeout => TimeSpan.FromSeconds(30);
    
    public IReadOnlyDictionary<string, object?> Parameters { get; }
    public IReadOnlyDictionary<string, object> Metadata { get; }
    public bool IsDataModifying => false;
    
    public UserQueryCommand(string? firstName = null, string? lastName = null)
    {
        var parameters = new Dictionary<string, object?>();
        if (!string.IsNullOrWhiteSpace(firstName))
            parameters["FirstName"] = firstName;
        if (!string.IsNullOrWhiteSpace(lastName))
            parameters["LastName"] = lastName;
            
        Parameters = parameters;
        Metadata = new Dictionary<string, object>
        {
            ["CacheKey"] = $"users_{firstName}_{lastName}",
            ["ReadPreference"] = "Secondary"
        };
    }
    
    public IFdwResult Validate()
    {
        // Add any command-specific validation
        return FdwResult.Success();
    }
    
    public IDataCommand WithParameters(IReadOnlyDictionary<string, object?> newParameters)
    {
        return new UserQueryCommand(); // Implementation would use new parameters
    }
    
    public IDataCommand WithMetadata(IReadOnlyDictionary<string, object> newMetadata)
    {
        return new UserQueryCommand(); // Implementation would use new metadata
    }
    
    public IDataCommand<List<User>> WithParameters(IReadOnlyDictionary<string, object?> newParameters)
    {
        return new UserQueryCommand(); // Implementation would use new parameters
    }
    
    public IDataCommand<List<User>> WithMetadata(IReadOnlyDictionary<string, object> newMetadata)
    {
        return new UserQueryCommand(); // Implementation would use new metadata
    }
}

// Using the data provider
public async Task<IFdwResult<List<User>>> GetUsersAsync()
{
    // Find a data provider that supports SQL queries
    var provider = DataProviders.GetByCommandType("Query")
        .FirstOrDefault(p => p.SupportedDataSources.Contains("SqlServer"));
        
    if (provider == null)
        return FdwResult<List<User>>.Failure("No SQL query provider found");
    
    // Execute the command
    var command = new UserQueryCommand("John", null);
    var result = await provider.Execute(command);
    
    return result;
}
```

### Creating a Custom Data Provider

```csharp
// Define configuration for your data provider
public sealed class JsonFileProviderConfiguration : FdwConfigurationBase
{
    public string BasePath { get; set; } = string.Empty;
    public bool EnableCaching { get; set; } = true;
    public int CacheExpirationMinutes { get; set; } = 30;
    
    public override IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(BasePath))
            errors.Add("Base path is required");
            
        if (!Directory.Exists(BasePath))
            errors.Add($"Base path '{BasePath}' does not exist");
            
        if (CacheExpirationMinutes <= 0)
            errors.Add("Cache expiration must be positive");
            
        return errors;
    }
}

// Implement the data provider
public sealed class JsonFileDataProvider : DataProviderBase, IDataProvider<JsonFileProviderConfiguration>
{
    private readonly IMemoryCache _cache;
    private JsonFileProviderConfiguration? _configuration;
    
    public JsonFileDataProvider(IMemoryCache cache) : base(
        id: 1,
        name: "JSON File Data Provider",
        supportedCommandTypes: new[] { "Query", "Insert", "Update", "Delete" },
        supportedDataSources: new[] { "JsonFile", "FileSystem" })
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }
    
    public JsonFileProviderConfiguration? Configuration => _configuration;
    
    public async Task<IFdwResult> InitializeAsync(JsonFileProviderConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        
        var validationErrors = configuration.Validate();
        if (validationErrors.Count > 0)
            return FdwResult.Failure("Configuration validation failed", validationErrors);
        
        _configuration = configuration;
        return FdwResult.Success();
    }
    
    public override async Task<IFdwResult<object?>> Execute(IDataCommand command)
    {
        if (!IsAvailable)
            return FdwResult<object?>.Failure("Provider is not available");
            
        var validationResult = ValidateCommand(command);
        if (validationResult.IsFailure)
            return FdwResult<object?>.Failure(validationResult.ErrorMessage);
        
        try
        {
            return command.CommandType switch
            {
                "Query" => await ExecuteQuery(command),
                "Insert" => await ExecuteInsert(command),
                "Update" => await ExecuteUpdate(command),
                "Delete" => await ExecuteDelete(command),
                _ => FdwResult<object?>.Failure($"Unsupported command type: {command.CommandType}")
            };
        }
        catch (Exception ex)
        {
            return FdwResult<object?>.Failure($"Command execution failed: {ex.Message}", ex);
        }
    }
    
    public override async Task<IFdwResult<TResult>> Execute<TResult>(IDataCommand<TResult> command)
    {
        var result = await Execute((IDataCommand)command);
        if (result.IsFailure)
            return FdwResult<TResult>.Failure(result.ErrorMessage, result.Exception);
        
        if (result.Value is TResult typedResult)
            return FdwResult<TResult>.Success(typedResult);
        
        try
        {
            var convertedResult = (TResult)Convert.ChangeType(result.Value, typeof(TResult));
            return FdwResult<TResult>.Success(convertedResult);
        }
        catch (Exception ex)
        {
            return FdwResult<TResult>.Failure($"Failed to convert result to {typeof(TResult).Name}", ex);
        }
    }
    
    private async Task<IFdwResult<object?>> ExecuteQuery(IDataCommand command)
    {
        if (command.Target == null)
            return FdwResult<object?>.Failure("Query target is required");
        
        var filePath = Path.Combine(_configuration!.BasePath, $"{command.Target}.json");
        if (!File.Exists(filePath))
            return FdwResult<object?>.Failure($"File not found: {filePath}");
        
        string cacheKey = $"file_{command.Target}_{command.Parameters.GetHashCode()}";
        
        // Check cache if enabled
        if (_configuration.EnableCaching && _cache.TryGetValue(cacheKey, out var cachedData))
        {
            return FdwResult<object?>.Success(cachedData);
        }
        
        // Read and deserialize file
        var json = await File.ReadAllTextAsync(filePath);
        var data = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json);
        
        // Apply filters based on parameters
        if (command.Parameters.Count > 0)
        {
            data = data?.Where(item => MatchesParameters(item, command.Parameters)).ToList();
        }
        
        // Cache the result if enabled
        if (_configuration.EnableCaching)
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_configuration.CacheExpirationMinutes)
            };
            _cache.Set(cacheKey, data, cacheOptions);
        }
        
        return FdwResult<object?>.Success(data);
    }
    
    private async Task<IFdwResult<object?>> ExecuteInsert(IDataCommand command)
    {
        if (command.Target == null)
            return FdwResult<object?>.Failure("Insert target is required");
        
        var filePath = Path.Combine(_configuration!.BasePath, $"{command.Target}.json");
        
        // Load existing data or create new list
        List<Dictionary<string, object>> data;
        if (File.Exists(filePath))
        {
            var json = await File.ReadAllTextAsync(filePath);
            data = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json) ?? new();
        }
        else
        {
            data = new List<Dictionary<string, object>>();
        }
        
        // Add new record
        var newRecord = command.Parameters.ToDictionary(kvp => kvp.Key, kvp => kvp.Value ?? new object());
        newRecord["Id"] = Guid.NewGuid().ToString();
        newRecord["CreatedAt"] = DateTime.UtcNow;
        
        data.Add(newRecord);
        
        // Save back to file
        var updatedJson = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(filePath, updatedJson);
        
        // Invalidate cache
        InvalidateCache(command.Target);
        
        return FdwResult<object?>.Success(newRecord);
    }
    
    private async Task<IFdwResult<object?>> ExecuteUpdate(IDataCommand command)
    {
        if (command.Target == null)
            return FdwResult<object?>.Failure("Update target is required");
        
        if (!command.Parameters.ContainsKey("Id"))
            return FdwResult<object?>.Failure("Update requires an Id parameter");
        
        var filePath = Path.Combine(_configuration!.BasePath, $"{command.Target}.json");
        if (!File.Exists(filePath))
            return FdwResult<object?>.Failure($"File not found: {filePath}");
        
        var json = await File.ReadAllTextAsync(filePath);
        var data = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json);
        
        if (data == null)
            return FdwResult<object?>.Failure("Failed to deserialize data");
        
        var recordId = command.Parameters["Id"]?.ToString();
        var record = data.FirstOrDefault(r => r.GetValueOrDefault("Id")?.ToString() == recordId);
        
        if (record == null)
            return FdwResult<object?>.Failure($"Record with Id '{recordId}' not found");
        
        // Update record
        foreach (var param in command.Parameters.Where(p => p.Key != "Id"))
        {
            record[param.Key] = param.Value ?? new object();
        }
        record["UpdatedAt"] = DateTime.UtcNow;
        
        // Save back to file
        var updatedJson = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(filePath, updatedJson);
        
        // Invalidate cache
        InvalidateCache(command.Target);
        
        return FdwResult<object?>.Success(record);
    }
    
    private async Task<IFdwResult<object?>> ExecuteDelete(IDataCommand command)
    {
        if (command.Target == null)
            return FdwResult<object?>.Failure("Delete target is required");
        
        if (!command.Parameters.ContainsKey("Id"))
            return FdwResult<object?>.Failure("Delete requires an Id parameter");
        
        var filePath = Path.Combine(_configuration!.BasePath, $"{command.Target}.json");
        if (!File.Exists(filePath))
            return FdwResult<object?>.Failure($"File not found: {filePath}");
        
        var json = await File.ReadAllTextAsync(filePath);
        var data = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json);
        
        if (data == null)
            return FdwResult<object?>.Failure("Failed to deserialize data");
        
        var recordId = command.Parameters["Id"]?.ToString();
        var recordIndex = data.FindIndex(r => r.GetValueOrDefault("Id")?.ToString() == recordId);
        
        if (recordIndex == -1)
            return FdwResult<object?>.Failure($"Record with Id '{recordId}' not found");
        
        var deletedRecord = data[recordIndex];
        data.RemoveAt(recordIndex);
        
        // Save back to file
        var updatedJson = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(filePath, updatedJson);
        
        // Invalidate cache
        InvalidateCache(command.Target);
        
        return FdwResult<object?>.Success(deletedRecord);
    }
    
    private bool MatchesParameters(Dictionary<string, object> item, IReadOnlyDictionary<string, object?> parameters)
    {
        foreach (var param in parameters)
        {
            if (!item.TryGetValue(param.Key, out var value))
                return false;
                
            if (!Equals(value, param.Value))
                return false;
        }
        return true;
    }
    
    private void InvalidateCache(string target)
    {
        // In a real implementation, you'd have a more sophisticated cache invalidation strategy
        // This is simplified for the example
    }
    
    public override async Task<IFdwResult<IProviderMetrics>> GetMetricsAsync()
    {
        var metrics = new JsonFileProviderMetrics
        {
            ProviderName = "JsonFileDataProvider",
            QueriesExecuted = 0, // Track in real implementation
            CommandsExecuted = 0,
            AverageExecutionTime = TimeSpan.Zero,
            CacheHitRate = 0.0,
            FilesAccessed = Directory.GetFiles(_configuration?.BasePath ?? "").Length
        };
        
        return FdwResult<IProviderMetrics>.Success(metrics);
    }
    
    public override async Task<IFdwResult<IDataTransaction>> BeginTransactionAsync()
    {
        // File-based providers typically don't support transactions
        return FdwResult<IDataTransaction>.Failure("Transactions are not supported by the JSON file provider");
    }
    
    protected override ICommandResult CreateCommandResult(IDataCommand command, int batchPosition, bool isSuccessful, 
        object? resultData, string? errorMessage, IReadOnlyList<string>? errorDetails, Exception? exception, 
        TimeSpan executionTime, DateTimeOffset startedAt, DateTimeOffset completedAt)
    {
        return new JsonFileCommandResult(command, batchPosition, isSuccessful, resultData, errorMessage, 
            errorDetails, exception, executionTime, startedAt, completedAt);
    }
    
    protected override IBatchResult CreateBatchResult(string batchId, int totalCommands, int successfulCommands, 
        int failedCommands, int skippedCommands, TimeSpan executionTime, DateTimeOffset startedAt, 
        DateTimeOffset completedAt, IReadOnlyList<ICommandResult> commandResults, IReadOnlyList<string> batchErrors)
    {
        return new JsonFileBatchResult(batchId, totalCommands, successfulCommands, failedCommands, skippedCommands,
            executionTime, startedAt, completedAt, commandResults, batchErrors);
    }
    
    public override void RegisterService(IServiceCollection services)
    {
        services.AddSingleton<JsonFileDataProvider>();
        services.AddSingleton<IDataProvider>(sp => sp.GetRequiredService<JsonFileDataProvider>());
        services.AddMemoryCache(); // Required for caching
    }
}
```

## Implementation Examples

### SQL Server Data Provider

```csharp
public sealed class SqlServerDataProvider : DataProviderBase, IDataProvider<SqlServerConfiguration>
{
    private readonly IExternalConnection _connection;
    private SqlServerConfiguration? _configuration;
    
    public SqlServerDataProvider(IExternalConnection connection) : base(
        id: 2,
        name: "SQL Server Data Provider",
        supportedCommandTypes: new[] { "Query", "Insert", "Update", "Delete", "StoredProcedure", "Bulk" },
        supportedDataSources: new[] { "SqlServer", "AzureSql" },
        connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }
    
    public SqlServerConfiguration? Configuration => _configuration;
    
    public async Task<IFdwResult> InitializeAsync(SqlServerConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        
        var validationErrors = configuration.Validate();
        if (validationErrors.Count > 0)
            return FdwResult.Failure("Configuration validation failed", validationErrors);
        
        _configuration = configuration;
        
        // Initialize connection if needed
        if (_connection.State != FdwConnectionState.Open)
        {
            var openResult = await _connection.OpenAsync();
            if (openResult.IsFailure)
                return FdwResult.Failure("Failed to open database connection", openResult.Exception);
        }
        
        return FdwResult.Success();
    }
    
    public override async Task<IFdwResult<object?>> Execute(IDataCommand command)
    {
        if (!IsAvailable)
            return FdwResult<object?>.Failure("Provider is not available");
            
        var validationResult = ValidateCommand(command);
        if (validationResult.IsFailure)
            return FdwResult<object?>.Failure(validationResult.ErrorMessage);
        
        try
        {
            using var sqlConnection = (SqlConnection)_connection;
            using var sqlCommand = new SqlCommand();
            sqlCommand.Connection = sqlConnection;
            sqlCommand.CommandTimeout = (int)(command.Timeout?.TotalSeconds ?? _configuration?.CommandTimeout ?? 30);
            
            // Configure command based on type
            switch (command.CommandType)
            {
                case "Query":
                    return await ExecuteSqlQuery(sqlCommand, command);
                case "Insert":
                    return await ExecuteSqlInsert(sqlCommand, command);
                case "Update":
                    return await ExecuteSqlUpdate(sqlCommand, command);
                case "Delete":
                    return await ExecuteSqlDelete(sqlCommand, command);
                case "StoredProcedure":
                    return await ExecuteStoredProcedure(sqlCommand, command);
                case "Bulk":
                    return await ExecuteBulkOperation(command);
                default:
                    return FdwResult<object?>.Failure($"Unsupported command type: {command.CommandType}");
            }
        }
        catch (SqlException ex)
        {
            return FdwResult<object?>.Failure($"SQL execution failed: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            return FdwResult<object?>.Failure($"Command execution failed: {ex.Message}", ex);
        }
    }
    
    private async Task<IFdwResult<object?>> ExecuteSqlQuery(SqlCommand sqlCommand, IDataCommand command)
    {
        // Build SELECT statement
        var query = BuildSelectQuery(command);
        sqlCommand.CommandText = query;
        
        // Add parameters
        foreach (var param in command.Parameters)
        {
            sqlCommand.Parameters.AddWithValue($"@{param.Key}", param.Value ?? DBNull.Value);
        }
        
        // Execute query
        using var reader = await sqlCommand.ExecuteReaderAsync();
        var results = new List<Dictionary<string, object>>();
        
        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                row[reader.GetName(i)] = reader.GetValue(i);
            }
            results.Add(row);
        }
        
        return FdwResult<object?>.Success(results);
    }
    
    private async Task<IFdwResult<object?>> ExecuteSqlInsert(SqlCommand sqlCommand, IDataCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Target))
            return FdwResult<object?>.Failure("Insert target table is required");
        
        // Build INSERT statement
        var columns = string.Join(", ", command.Parameters.Keys);
        var values = string.Join(", ", command.Parameters.Keys.Select(k => $"@{k}"));
        sqlCommand.CommandText = $"INSERT INTO {command.Target} ({columns}) OUTPUT INSERTED.* VALUES ({values})";
        
        // Add parameters
        foreach (var param in command.Parameters)
        {
            sqlCommand.Parameters.AddWithValue($"@{param.Key}", param.Value ?? DBNull.Value);
        }
        
        // Execute and return inserted record
        using var reader = await sqlCommand.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var insertedRecord = new Dictionary<string, object>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                insertedRecord[reader.GetName(i)] = reader.GetValue(i);
            }
            return FdwResult<object?>.Success(insertedRecord);
        }
        
        return FdwResult<object?>.Failure("Insert operation did not return a record");
    }
    
    private string BuildSelectQuery(IDataCommand command)
    {
        var query = $"SELECT * FROM {command.Target}";
        
        if (command.Parameters.Count > 0)
        {
            var whereClause = string.Join(" AND ", command.Parameters.Keys.Select(k => $"{k} = @{k}"));
            query += $" WHERE {whereClause}";
        }
        
        return query;
    }
    
    public override async Task<IFdwResult<IDataTransaction>> BeginTransactionAsync()
    {
        try
        {
            var transaction = new SqlServerDataTransaction(_connection as SqlConnection, this);
            await transaction.BeginAsync();
            return FdwResult<IDataTransaction>.Success(transaction);
        }
        catch (Exception ex)
        {
            return FdwResult<IDataTransaction>.Failure("Failed to begin transaction", ex);
        }
    }
    
    // Additional implementation methods...
}
```

### MongoDB Data Provider

```csharp
public sealed class MongoDbDataProvider : DataProviderBase, IDataProvider<MongoDbConfiguration>
{
    private readonly IMongoDatabase _database;
    private MongoDbConfiguration? _configuration;
    
    public MongoDbDataProvider(IMongoDatabase database) : base(
        id: 3,
        name: "MongoDB Data Provider",
        supportedCommandTypes: new[] { "Query", "Insert", "Update", "Delete", "Aggregate" },
        supportedDataSources: new[] { "MongoDB", "DocumentDb" })
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }
    
    public MongoDbConfiguration? Configuration => _configuration;
    
    public override async Task<IFdwResult<object?>> Execute(IDataCommand command)
    {
        if (!IsAvailable)
            return FdwResult<object?>.Failure("Provider is not available");
            
        try
        {
            return command.CommandType switch
            {
                "Query" => await ExecuteMongoQuery(command),
                "Insert" => await ExecuteMongoInsert(command),
                "Update" => await ExecuteMongoUpdate(command),
                "Delete" => await ExecuteMongoDelete(command),
                "Aggregate" => await ExecuteMongoAggregate(command),
                _ => FdwResult<object?>.Failure($"Unsupported command type: {command.CommandType}")
            };
        }
        catch (MongoException ex)
        {
            return FdwResult<object?>.Failure($"MongoDB operation failed: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            return FdwResult<object?>.Failure($"Command execution failed: {ex.Message}", ex);
        }
    }
    
    private async Task<IFdwResult<object?>> ExecuteMongoQuery(IDataCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Target))
            return FdwResult<object?>.Failure("Query target collection is required");
        
        var collection = _database.GetCollection<BsonDocument>(command.Target);
        
        // Build filter from parameters
        var filterBuilder = Builders<BsonDocument>.Filter;
        var filter = FilterDefinition<BsonDocument>.Empty;
        
        foreach (var param in command.Parameters)
        {
            if (param.Value != null)
            {
                filter &= filterBuilder.Eq(param.Key, BsonValue.Create(param.Value));
            }
        }
        
        // Execute query
        var cursor = await collection.FindAsync(filter);
        var documents = await cursor.ToListAsync();
        
        // Convert to dictionary format for consistency
        var results = documents.Select(doc => doc.ToDictionary()).ToList();
        
        return FdwResult<object?>.Success(results);
    }
    
    private async Task<IFdwResult<object?>> ExecuteMongoInsert(IDataCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Target))
            return FdwResult<object?>.Failure("Insert target collection is required");
        
        var collection = _database.GetCollection<BsonDocument>(command.Target);
        
        // Create document from parameters  
        var document = new BsonDocument();
        foreach (var param in command.Parameters)
        {
            document[param.Key] = BsonValue.Create(param.Value);
        }
        
        // Add metadata
        document["_createdAt"] = DateTime.UtcNow;
        
        // Insert document
        await collection.InsertOneAsync(document);
        
        return FdwResult<object?>.Success(document.ToDictionary());
    }
    
    // Additional MongoDB implementation methods...
}
```

### REST API Data Provider

```csharp
public sealed class ApiDataProvider : DataProviderBase, IDataProvider<ApiConfiguration>
{
    private readonly HttpClient _httpClient;
    private ApiConfiguration? _configuration;
    
    public ApiDataProvider(HttpClient httpClient) : base(
        id: 4,
        name: "REST API Data Provider", 
        supportedCommandTypes: new[] { "Get", "Post", "Put", "Delete" },
        supportedDataSources: new[] { "RestApi", "WebApi", "JsonApi" })
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }
    
    public ApiConfiguration? Configuration => _configuration;
    
    public override async Task<IFdwResult<object?>> Execute(IDataCommand command)
    {
        if (!IsAvailable)
            return FdwResult<object?>.Failure("Provider is not available");
            
        try
        {
            return command.CommandType switch
            {
                "Get" => await ExecuteGetRequest(command),
                "Post" => await ExecutePostRequest(command),
                "Put" => await ExecutePutRequest(command),
                "Delete" => await ExecuteDeleteRequest(command),
                _ => FdwResult<object?>.Failure($"Unsupported command type: {command.CommandType}")
            };
        }
        catch (HttpRequestException ex)
        {
            return FdwResult<object?>.Failure($"HTTP request failed: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            return FdwResult<object?>.Failure($"Command execution failed: {ex.Message}", ex);
        }
    }
    
    private async Task<IFdwResult<object?>> ExecuteGetRequest(IDataCommand command)
    {
        var url = BuildUrl(command);
        var response = await _httpClient.GetAsync(url);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<object>(content);
            return FdwResult<object?>.Success(data);
        }
        
        return FdwResult<object?>.Failure($"GET request failed: {response.StatusCode}");
    }
    
    private async Task<IFdwResult<object?>> ExecutePostRequest(IDataCommand command)
    {
        var url = BuildUrl(command);
        var json = JsonSerializer.Serialize(command.Parameters);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync(url, content);
        
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<object>(responseContent);
            return FdwResult<object?>.Success(data);
        }
        
        return FdwResult<object?>.Failure($"POST request failed: {response.StatusCode}");
    }
    
    private string BuildUrl(IDataCommand command)
    {
        var baseUrl = command.Target ?? "";
        
        if (command.CommandType == "Get" && command.Parameters.Count > 0)
        {
            var queryString = string.Join("&", command.Parameters.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value?.ToString() ?? "")}"));
            return $"{baseUrl}?{queryString}";
        }
        
        return baseUrl;
    }
    
    // Additional API implementation methods...
}
```

## Configuration Examples  

### JSON Configuration for Multiple Data Providers

```json
{
  "DataProviders": {
    "PrimaryDatabase": {
      "Provider": "SqlServer",
      "ConnectionString": "Server=localhost;Database=MyApp;Trusted_Connection=true;",
      "CommandTimeout": 60,
      "EnableRetryLogic": true,
      "MaxRetryCount": 3
    },
    "DocumentStore": {
      "Provider": "MongoDB",
      "ConnectionString": "mongodb://localhost:27017",
      "DatabaseName": "MyAppDocs",
      "MaxConnectionPoolSize": 50
    },
    "FileStorage": {
      "Provider": "JsonFile",
      "BasePath": "C:\\AppData\\JsonFiles",
      "EnableCaching": true,
      "CacheExpirationMinutes": 30
    },
    "ExternalApi": {
      "Provider": "RestApi",
      "BaseUrl": "https://api.external.com/v1",
      "ApiKey": "your-api-key",
      "TimeoutSeconds": 45
    }
  }
}
```

### Dependency Injection Setup

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Configure data provider settings
    services.Configure<SqlServerConfiguration>(
        Configuration.GetSection("DataProviders:PrimaryDatabase"));
    services.Configure<MongoDbConfiguration>(
        Configuration.GetSection("DataProviders:DocumentStore"));
    services.Configure<JsonFileProviderConfiguration>(
        Configuration.GetSection("DataProviders:FileStorage"));
    
    // Register connections
    services.AddScoped<IExternalConnection, SqlServerConnection>();
    services.AddSingleton<IMongoDatabase>(sp => 
    {
        var config = sp.GetRequiredService<IOptions<MongoDbConfiguration>>().Value;
        var client = new MongoClient(config.ConnectionString);
        return client.GetDatabase(config.DatabaseName);
    });
    
    // Register data providers (auto-discovered via DataProviders collection)
    services.AddDataProviders();
    
    // Register application services that use data providers
    services.AddScoped<IUserService, UserService>();
    services.AddScoped<IOrderService, OrderService>();
}

// Extension method for bulk registration
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataProviders(this IServiceCollection services)
    {
        // Register all discovered data providers
        foreach (var provider in DataProviders.All)
        {
            provider.RegisterService(services);
        }
        
        return services;
    }
}
```

## Advanced Usage

### Transaction Management

```csharp
public sealed class OrderService
{
    private readonly IDataProvider _sqlProvider;
    private readonly IDataProvider _mongoProvider;
    
    public OrderService(
        [FromKeyedServices("SqlServer")] IDataProvider sqlProvider,
        [FromKeyedServices("MongoDB")] IDataProvider mongoProvider)
    {
        _sqlProvider = sqlProvider;
        _mongoProvider = mongoProvider;
    }
    
    public async Task<IFdwResult<Order>> CreateOrderAsync(CreateOrderRequest request)
    {
        // Start transaction for SQL operations
        var transactionResult = await _sqlProvider.BeginTransactionAsync();
        if (transactionResult.IsFailure)
            return FdwResult<Order>.Failure("Failed to begin transaction");
        
        var transaction = transactionResult.Value;
        
        try
        {
            // Insert order record
            var insertOrderCommand = new InsertCommand<Order>
            {
                Target = "Orders",
                Parameters = new Dictionary<string, object?>
                {
                    ["CustomerId"] = request.CustomerId,
                    ["TotalAmount"] = request.TotalAmount,
                    ["Status"] = "Pending",
                    ["CreatedAt"] = DateTime.UtcNow
                }
            };
            
            var orderResult = await transaction.Execute(insertOrderCommand);
            if (orderResult.IsFailure)
            {
                await transaction.RollbackAsync();
                return FdwResult<Order>.Failure("Failed to create order", orderResult.Exception);
            }
            
            var order = (Order)orderResult.Value!;
            
            // Insert order items
            foreach (var item in request.Items)
            {
                var insertItemCommand = new InsertCommand
                {
                    Target = "OrderItems",
                    Parameters = new Dictionary<string, object?>
                    {
                        ["OrderId"] = order.Id,
                        ["ProductId"] = item.ProductId,
                        ["Quantity"] = item.Quantity,
                        ["UnitPrice"] = item.UnitPrice
                    }
                };
                
                var itemResult = await transaction.Execute(insertItemCommand);
                if (itemResult.IsFailure)
                {
                    await transaction.RollbackAsync();
                    return FdwResult<Order>.Failure("Failed to create order item", itemResult.Exception);
                }
            }
            
            // Commit SQL transaction
            var commitResult = await transaction.CommitAsync();
            if (commitResult.IsFailure)
            {
                await transaction.RollbackAsync();
                return FdwResult<Order>.Failure("Failed to commit transaction");
            }
            
            // Store order summary in MongoDB (outside transaction)
            var orderSummaryCommand = new InsertCommand
            {
                Target = "OrderSummaries",
                Parameters = new Dictionary<string, object?>
                {
                    ["OrderId"] = order.Id,
                    ["CustomerId"] = order.CustomerId,
                    ["ItemCount"] = request.Items.Count,
                    ["TotalAmount"] = order.TotalAmount,
                    ["CreatedAt"] = order.CreatedAt
                }
            };
            
            await _mongoProvider.Execute(orderSummaryCommand);
            
            return FdwResult<Order>.Success(order);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return FdwResult<Order>.Failure("Order creation failed", ex);
        }
        finally
        {
            transaction.Dispose();
        }
    }
}
```

### Batch Processing

```csharp
public sealed class BulkDataProcessor
{
    private readonly IDataProvider _dataProvider;
    
    public BulkDataProcessor(IDataProvider dataProvider)
    {
        _dataProvider = dataProvider;
    }
    
    public async Task<IFdwResult<IBatchResult>> ProcessBulkImport(IReadOnlyList<ImportRecord> records)
    {
        // Create batch of insert commands
        var commands = records.Select(record => new InsertCommand
        {
            Target = "ImportedData",
            Parameters = new Dictionary<string, object?>
            {
                ["ExternalId"] = record.ExternalId,
                ["Name"] = record.Name,
                ["Value"] = record.Value,
                ["ImportedAt"] = DateTime.UtcNow
            }
        }).Cast<IDataCommand>().ToList();
        
        // Execute as batch
        var batchResult = await _dataProvider.ExecuteBatch(commands);
        
        if (batchResult.IsSuccess)
        {
            var result = batchResult.Value;
            Console.WriteLine($"Batch processed: {result.SuccessfulCommands}/{result.TotalCommands} successful");
            
            if (result.FailedCommands > 0)
            {
                Console.WriteLine($"Failed commands:");
                foreach (var failedCommand in result.CommandResults.Where(r => !r.IsSuccessful))
                {
                    Console.WriteLine($"  - Position {failedCommand.BatchPosition}: {failedCommand.ErrorMessage}");
                }
            }
        }
        
        return batchResult;
    }
}
```

### Provider Selection and Routing

```csharp
public sealed class DataProviderRouter
{
    private readonly IServiceProvider _serviceProvider;
    
    public DataProviderRouter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public async Task<IFdwResult<object?>> ExecuteCommand(IDataCommand command, string? preferredDataSource = null)
    {
        // Find providers that support this command type
        var candidateProviders = DataProviders.GetByCommandType(command.CommandType);
        
        if (preferredDataSource != null)
        {
            // Filter by preferred data source
            candidateProviders = candidateProviders.Where(p => p.SupportedDataSources.Contains(preferredDataSource));
        }
        
        // Sort by priority (highest first)
        var orderedProviders = candidateProviders.OrderByDescending(p => p.Priority);
        
        foreach (var providerInfo in orderedProviders)
        {
            try
            {
                // Get provider instance
                var provider = await providerInfo.CreateService(_serviceProvider);
                if (provider.IsFailure)
                    continue;
                
                var dataProvider = provider.Value;
                
                // Check if provider is available
                if (!dataProvider.IsAvailable)
                    continue;
                
                // Validate command against provider
                var validationResult = dataProvider.ValidateCommand(command);
                if (validationResult.IsFailure)
                    continue;
                
                // Execute command
                return await dataProvider.Execute(command);
            }
            catch (Exception ex)
            {
                // Log error and try next provider
                Console.WriteLine($"Provider {providerInfo.Name} failed: {ex.Message}");
                continue;
            }
        }
        
        return FdwResult<object?>.Failure($"No available provider found for command type '{command.CommandType}'");
    }
}
```

### Performance Monitoring

```csharp
public sealed class DataProviderMonitor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DataProviderMonitor> _logger;
    
    public DataProviderMonitor(IServiceProvider serviceProvider, ILogger<DataProviderMonitor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await CollectMetrics();
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
    
    private async Task CollectMetrics()
    {
        foreach (var providerInfo in DataProviders.All)
        {
            try
            {
                var providerResult = await providerInfo.CreateService(_serviceProvider);
                if (providerResult.IsFailure)
                    continue;
                    
                var provider = providerResult.Value;
                var metricsResult = await provider.GetMetricsAsync();
                
                if (metricsResult.IsSuccess)
                {
                    var metrics = metricsResult.Value;
                    _logger.LogInformation("Provider {Provider} metrics: Commands={Commands}, AvgTime={AvgTime}ms", 
                        providerInfo.Name, metrics.CommandsExecuted, metrics.AverageExecutionTime.TotalMilliseconds);
                        
                    // Send metrics to monitoring system
                    await SendMetricsToMonitoring(providerInfo.Name, metrics);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to collect metrics for provider {Provider}", providerInfo.Name);
            }
        }
    }
    
    private async Task SendMetricsToMonitoring(string providerName, IProviderMetrics metrics)
    {
        // Send to your preferred monitoring system (Prometheus, Application Insights, etc.)
    }
}
```

## Best Practices

1. **Use typed commands** when the result type is known at compile time
2. **Implement proper validation** in both commands and providers
3. **Handle transactions carefully** - always dispose and consider rollback scenarios
4. **Use batch operations** for bulk data processing
5. **Monitor provider performance** and health proactively
6. **Cache frequently accessed data** when appropriate
7. **Implement retry logic** for transient failures
8. **Route commands intelligently** based on provider capabilities
9. **Log command execution** for debugging and auditing
10. **Test provider fallback scenarios** to ensure resilience

## Integration with Other Framework Components

This abstraction layer works seamlessly with other FractalDataWorks packages:

- **ExternalConnections**: Data providers use connections for database access
- **Authentication**: Commands can include authentication context
- **SecretManagement**: Connection strings and API keys are managed securely
- **Transformations**: Transform data between different provider formats
- **Scheduling**: Schedule batch data operations

## License

This package is part of the FractalDataWorks Framework and is licensed under the Apache 2.0 License.