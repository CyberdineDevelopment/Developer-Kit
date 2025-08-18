# FractalDataWorks.Configuration

Configuration management patterns and providers for the FractalDataWorks framework. This package provides base classes and abstractions for creating self-validating configurations with provider patterns.

## Overview

FractalDataWorks.Configuration provides:
- Self-validating configuration base class
- Configuration provider pattern for loading from various sources
- Configuration source abstractions
- Built-in validation using FluentValidation
- Integration with Microsoft.Extensions.Configuration

## Key Components

### ConfigurationBase<T>

Base class for creating self-validating configurations:

```csharp
public abstract class ConfigurationBase<T> : IFdwConfiguration
    where T : ConfigurationBase<T>, new()
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public bool IsEnabled { get; set; } = true;
    public bool IsDefault { get; set; }
    
    // Override to provide custom validation
    protected abstract ValidationResult ValidateCore();
    
    // Validates and caches the result
    public bool IsValid { get; }
    public ValidationResult? ValidationResult { get; }
}
```

### ConfigurationProviderBase

Base class for implementing configuration providers:

```csharp
public abstract class ConfigurationProviderBase : IConfigurationProvider
{
    protected ConfigurationProviderBase(ILogger logger);
    
    // Override these to implement your provider
    protected abstract Task<IEnumerable<IFdwConfiguration>> LoadConfigurationsAsync();
    protected abstract Task SaveConfigurationAsync(IFdwConfiguration configuration);
    
    // Built-in features
    public async Task<T?> GetAsync<T>(int id) where T : IFdwConfiguration;
    public async Task<IEnumerable<T>> GetAllAsync<T>() where T : IFdwConfiguration;
    public async Task<IFdwResult> SaveAsync(IFdwConfiguration configuration);
}
```

### ConfigurationSourceBase

Base class for configuration sources (integrates with Microsoft.Extensions.Configuration):

```csharp
public abstract class ConfigurationSourceBase : IConfigurationSource
{
    protected ConfigurationSourceBase(string name, ILogger logger);
    
    // Override to load your configuration data
    protected abstract Task<IDictionary<string, string?>> LoadAsync();
    
    public IConfigurationProvider Build(IConfigurationBuilder builder);
}
```

## Usage Examples

### Creating a Configuration

```csharp
public class DatabaseConfiguration : ConfigurationBase<DatabaseConfiguration>
{
    public string ConnectionString { get; set; } = string.Empty;
    public int CommandTimeout { get; set; } = 30;
    public int MaxRetries { get; set; } = 3;
    public bool EnableLogging { get; set; } = true;
    
    protected override ValidationResult ValidateCore()
    {
        var validator = new DatabaseConfigurationValidator();
        return validator.Validate(this);
    }
}

public class DatabaseConfigurationValidator : AbstractValidator<DatabaseConfiguration>
{
    public DatabaseConfigurationValidator()
    {
        RuleFor(x => x.ConnectionString)
            .NotEmpty().WithMessage("Connection string is required")
            .Must(BeValidConnectionString).WithMessage("Invalid connection string format");
            
        RuleFor(x => x.CommandTimeout)
            .InclusiveBetween(1, 300).WithMessage("Command timeout must be between 1 and 300 seconds");
            
        RuleFor(x => x.MaxRetries)
            .InclusiveBetween(0, 10).WithMessage("Max retries must be between 0 and 10");
    }
    
    private bool BeValidConnectionString(string connectionString)
    {
        try
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            return !string.IsNullOrEmpty(builder.DataSource);
        }
        catch
        {
            return false;
        }
    }
}
```

### Creating a Configuration Provider

```csharp
public class JsonConfigurationProvider : ConfigurationProviderBase
{
    private readonly string _filePath;
    
    public JsonConfigurationProvider(string filePath, ILogger<JsonConfigurationProvider> logger)
        : base(logger)
    {
        _filePath = filePath;
    }
    
    protected override async Task<IEnumerable<IFdwConfiguration>> LoadConfigurationsAsync()
    {
        if (!File.Exists(_filePath))
            return Enumerable.Empty<IFractalConfiguration>();
            
        var json = await File.ReadAllTextAsync(_filePath);
        var configs = JsonSerializer.Deserialize<List<DatabaseConfiguration>>(json);
        
        return configs ?? Enumerable.Empty<IFdwConfiguration>();
    }
    
    protected override async Task SaveConfigurationAsync(IFdwConfiguration configuration)
    {
        var configs = (await LoadConfigurationsAsync()).ToList();
        
        // Update or add configuration
        var existing = configs.FirstOrDefault(c => c.Id == configuration.Id);
        if (existing != null)
            configs.Remove(existing);
        configs.Add(configuration);
        
        var json = JsonSerializer.Serialize(configs, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        await File.WriteAllTextAsync(_filePath, json);
    }
}
```

### Using with Microsoft.Extensions.Configuration

```csharp
public class DatabaseConfigurationSource : ConfigurationSourceBase
{
    private readonly string _connectionString;
    
    public DatabaseConfigurationSource(string connectionString, ILogger logger)
        : base("Database", logger)
    {
        _connectionString = connectionString;
    }
    
    protected override async Task<IDictionary<string, string?>> LoadAsync()
    {
        var settings = new Dictionary<string, string?>();
        
        // Load from database
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        using var command = new SqlCommand("SELECT [Key], [Value] FROM Settings", connection);
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            settings[reader.GetString(0)] = reader.GetString(1);
        }
        
        return settings;
    }
}

// In Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    var configuration = new ConfigurationBuilder()
        .Add(new DatabaseConfigurationSource(connectionString, logger))
        .Build();
        
    services.Configure<DatabaseConfiguration>(configuration.GetSection("Database"));
}
```

### Validation Examples

```csharp
var config = new DatabaseConfiguration
{
    ConnectionString = "Server=localhost;Database=MyApp;",
    CommandTimeout = 60,
    MaxRetries = 3
};

// Automatic validation on IsValid check
if (config.IsValid)
{
    Console.WriteLine("Configuration is valid");
}
else
{
    Console.WriteLine("Configuration errors:");
    foreach (var error in config.ValidationResult.Errors)
    {
        Console.WriteLine($"- {error.PropertyName}: {error.ErrorMessage}");
    }
}
```

## Integration with Services

Configurations work seamlessly with the service pattern:

```csharp
public class MyService : ServiceBase<MyCommand, DatabaseConfiguration, MyService>
{
    public MyService(ILogger<MyService> logger, DatabaseConfiguration configuration)
        : base(logger, configuration)
    {
        // Configuration is automatically validated
        // Access via this.Configuration
    }
}
```

## Advanced Features

### Configuration Inheritance

Create hierarchical configurations:

```csharp
public abstract class CloudConfiguration : ConfigurationBase<CloudConfiguration>
{
    public string Region { get; set; } = "us-east-1";
    public string AccessKey { get; set; } = string.Empty;
}

public class S3Configuration : CloudConfiguration
{
    public string BucketName { get; set; } = string.Empty;
    public bool EnableVersioning { get; set; } = true;
    
    protected override ValidationResult ValidateCore()
    {
        // Validate both base and derived properties
        return new S3ConfigurationValidator().Validate(this);
    }
}
```

### Configuration Composition

Compose complex configurations:

```csharp
public class ApplicationConfiguration : ConfigurationBase<ApplicationConfiguration>
{
    public DatabaseConfiguration Database { get; set; } = new();
    public S3Configuration Storage { get; set; } = new();
    public LoggingConfiguration Logging { get; set; } = new();
    
    protected override ValidationResult ValidateCore()
    {
        var validator = new ApplicationConfigurationValidator();
        return validator.Validate(this);
    }
}
```

### Dynamic Configuration Reloading

```csharp
public class ReloadableConfigurationProvider : ConfigurationProviderBase
{
    private readonly IDisposable _changeToken;
    
    public ReloadableConfigurationProvider(IConfiguration configuration, ILogger logger)
        : base(logger)
    {
        _changeToken = ChangeToken.OnChange(
            () => configuration.GetReloadToken(),
            async () => await ReloadConfigurations());
    }
    
    private async Task ReloadConfigurations()
    {
        var configs = await LoadConfigurationsAsync();
        OnConfigurationsReloaded?.Invoke(configs);
    }
    
    public event Action<IEnumerable<IFdwConfiguration>>? OnConfigurationsReloaded;
}
```

## Installation

```xml
<PackageReference Include="FractalDataWorks.Configuration" Version="*" />
```

## Dependencies

- FractalDataWorks.Services (core abstractions)
- Microsoft.Extensions.Configuration
- Microsoft.Extensions.Configuration.Binder
- Microsoft.Extensions.Logging.Abstractions
- Microsoft.Extensions.Options
- Microsoft.Extensions.DependencyInjection.Abstractions
- FluentValidation

## Best Practices

1. **Always Validate**: Use FluentValidation for comprehensive validation rules
2. **Cache Validation Results**: The base class caches validation for performance
3. **Use Immutable Defaults**: Set sensible defaults in property initializers
4. **Version Configurations**: Use the Version property for migration scenarios
5. **Enable/Disable Logic**: Use IsEnabled for feature flags and gradual rollouts

## Testing

```csharp
[Fact]
public void Configuration_Should_Be_Invalid_With_Empty_ConnectionString()
{
    var config = new DatabaseConfiguration
    {
        ConnectionString = string.Empty
    };
    
    Assert.False(config.IsValid);
    Assert.Contains(config.ValidationResult.Errors, 
        e => e.PropertyName == nameof(DatabaseConfiguration.ConnectionString));
}

[Fact]
public async Task Provider_Should_Load_Configurations()
{
    var provider = new JsonConfigurationProvider("test-config.json", logger);
    var configs = await provider.GetAllAsync<DatabaseConfiguration>();
    
    Assert.NotEmpty(configs);
    Assert.All(configs, c => Assert.True(c.IsValid));
}
```