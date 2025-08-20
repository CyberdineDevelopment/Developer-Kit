# FractalDataWorks.Configuration

Configuration management patterns and providers for the FractalDataWorks framework. This package provides base classes and abstractions for creating self-validating configurations with provider patterns.

## Overview

FractalDataWorks.Configuration provides:
- Self-validating configuration base classes (ConfigurationBase<T> and FdwConfigurationBase<T>)
- Configuration provider pattern for loading from various sources
- Configuration source abstractions
- Built-in validation using FluentValidation with FluentValidation.Results.ValidationResult
- Integration with Microsoft.Extensions.Configuration

## Key Components

### FdwConfigurationBase<T> (Recommended)

New base class for creating self-validating configurations using FluentValidation.Results.ValidationResult:

```csharp
public abstract class FdwConfigurationBase<T> : IFdwConfiguration
    where T : FdwConfigurationBase<T>, new()
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public bool IsEnabled { get; set; } = true;
    public bool IsDefault { get; set; }
    
    // Override to provide FluentValidation validator
    protected abstract IValidator<T>? GetValidator();
    
    // Returns standard FluentValidation result
    public FluentValidation.Results.ValidationResult Validate()
    {
        var validator = GetValidator();
        return validator?.Validate((T)this) ?? new FluentValidation.Results.ValidationResult();
    }
}
```

### ConfigurationBase<T> (Legacy)

Legacy base class that returns IFdwResult (deprecated, use FdwConfigurationBase<T>):

```csharp
public abstract class ConfigurationBase<T> : IFdwConfiguration
    where T : ConfigurationBase<T>, new()
{
    // Returns IFdwResult for backward compatibility
    protected abstract ValidationResult ValidateCore();
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

### Creating a Configuration (Recommended Approach)

Using FdwConfigurationBase<T> with FluentValidation.Results.ValidationResult:

```csharp
public class DatabaseConfiguration : FdwConfigurationBase<DatabaseConfiguration>
{
    public string ConnectionString { get; set; } = string.Empty;
    public int CommandTimeout { get; set; } = 30;
    public int MaxRetries { get; set; } = 3;
    public bool EnableLogging { get; set; } = true;
    
    protected override IValidator<DatabaseConfiguration>? GetValidator()
    {
        return new DatabaseConfigurationValidator();
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

### Legacy Configuration (Using ConfigurationBase<T>)

```csharp
// Deprecated - use FdwConfigurationBase<T> instead
public class LegacyDatabaseConfiguration : ConfigurationBase<LegacyDatabaseConfiguration>
{
    protected override ValidationResult ValidateCore()
    {
        var validator = new DatabaseConfigurationValidator();
        return validator.Validate(this);
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

Using FdwConfigurationBase<T> with FluentValidation.Results.ValidationResult:

```csharp
var config = new DatabaseConfiguration
{
    ConnectionString = "Server=localhost;Database=MyApp;",
    CommandTimeout = 60,
    MaxRetries = 3
};

// Validate using FluentValidation result
var validationResult = config.Validate();
if (validationResult.IsValid)
{
    Console.WriteLine("Configuration is valid");
}
else
{
    Console.WriteLine("Configuration errors:");
    foreach (var error in validationResult.Errors)
    {
        Console.WriteLine($"- {error.PropertyName}: {error.ErrorMessage}");
    }
}
```

Legacy validation (ConfigurationBase<T>):

```csharp
var legacyConfig = new LegacyDatabaseConfiguration();

// Legacy approach using IsValid property
if (legacyConfig.IsValid)
{
    Console.WriteLine("Configuration is valid");
}
else
{
    Console.WriteLine("Configuration errors:");
    foreach (var error in legacyConfig.ValidationResult.Errors)
    {
        Console.WriteLine($"- {error.PropertyName}: {error.ErrorMessage}");
    }
}
```

## Integration with Services

Configurations work seamlessly with the service pattern. The recommended approach is to use FdwConfigurationBase<T>:

```csharp
public class MyService : ServiceBase<MyCommand, DatabaseConfiguration, MyService>
{
    public MyService(ILogger<MyService> logger, DatabaseConfiguration configuration)
        : base(logger, configuration)
    {
        // Configuration validation using FluentValidation.Results.ValidationResult
        var validationResult = configuration.Validate();
        if (!validationResult.IsValid)
        {
            // Handle validation errors appropriately
            var errors = string.Join("; ", validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
            throw new ArgumentException($"Invalid configuration: {errors}");
        }
        
        // Access via this.Configuration
    }
}
```

Legacy approach (ConfigurationBase<T>):

```csharp
public class LegacyService : ServiceBase<MyCommand, LegacyDatabaseConfiguration, LegacyService>
{
    public LegacyService(ILogger<LegacyService> logger, LegacyDatabaseConfiguration configuration)
        : base(logger, configuration)
    {
        // Legacy validation approach
        if (!configuration.IsValid)
        {
            // Handle validation errors
        }
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

- FractalDataWorks.Configuration.Abstractions (configuration interfaces)
- FractalDataWorks.Results (result patterns)
- Microsoft.Extensions.Configuration
- Microsoft.Extensions.Configuration.Binder
- Microsoft.Extensions.Logging.Abstractions
- Microsoft.Extensions.Options
- Microsoft.Extensions.DependencyInjection.Abstractions
- FluentValidation

## Best Practices

1. **Use FdwConfigurationBase<T>**: Prefer the new base class with FluentValidation.Results.ValidationResult
2. **Always Validate**: Use FluentValidation for comprehensive validation rules
3. **Handle Validation Results**: Check validation results before using configurations
4. **Use Immutable Defaults**: Set sensible defaults in property initializers
5. **Version Configurations**: Use the Version property for migration scenarios
6. **Enable/Disable Logic**: Use IsEnabled for feature flags and gradual rollouts
7. **Avoid ConfigurationBase<T>**: The legacy base class is deprecated

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