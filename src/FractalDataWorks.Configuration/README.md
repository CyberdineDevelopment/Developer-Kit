# FractalDataWorks.Configuration

Configuration management package with full Microsoft.Extensions.Configuration integration. Provides strongly-typed, self-validating configuration classes that work seamlessly with .NET configuration systems.

## 📦 Package Information

- **Package ID**: `FractalDataWorks.Configuration`
- **Target Framework**: .NET Standard 2.0
- **Dependencies**: 
  - `FractalDataWorks` (core)
  - `Microsoft.Extensions.Configuration.Abstractions`
  - `Microsoft.Extensions.Options`
- **License**: Apache 2.0

## 🎯 Purpose

This package extends the core configuration functionality with:

- **IConfiguration Integration**: Seamless binding to .NET configuration
- **Options Pattern Support**: Full integration with IOptions<T>
- **Validation Integration**: DataAnnotations and custom validation
- **Environment-Specific Configs**: Development, staging, production configurations
- **Configuration Builders**: Fluent configuration building
- **Hot Reload Support**: Configuration changes at runtime

## 🚀 Usage

### Install Package

```bash
dotnet add package FractalDataWorks.Configuration
```

### Define Configuration

```csharp
using FractalDataWorks.Configuration;
using System.ComponentModel.DataAnnotations;

public class DatabaseConfiguration : EnhancedConfiguration<DatabaseConfiguration>
{
    [Required]
    public string ConnectionString { get; set; } = string.Empty;
    
    [Range(1, 300)]
    public int CommandTimeout { get; set; } = 30;
    
    [Range(1, 100)]
    public int MaxPoolSize { get; set; } = 10;
    
    public bool EnableRetryOnFailure { get; set; } = true;
    
    public override DatabaseConfiguration CreateDefault() => new()
    {
        ConnectionString = "Server=localhost;Database=DefaultDb;Integrated Security=true;",
        CommandTimeout = 30,
        MaxPoolSize = 10,
        EnableRetryOnFailure = true
    };
}
```

### Configuration Binding

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Automatic binding with validation
builder.Services.ConfigureFractalDataWorks<DatabaseConfiguration>(
    builder.Configuration.GetSection("Database"));

// With validation on startup
builder.Services.PostConfigure<DatabaseConfiguration>(config =>
{
    var validation = config.Validate();
    if (!validation.IsValid)
        throw new InvalidOperationException($"Database configuration invalid: {validation.AllErrors}");
});

var app = builder.Build();
```

### appsettings.json

```json
{
  "Database": {
    "ConnectionString": "Server=localhost;Database=MyApp;Integrated Security=true;",
    "CommandTimeout": 60,
    "MaxPoolSize": 20,
    "EnableRetryOnFailure": true
  },
  "Redis": {
    "ConnectionString": "localhost:6379",
    "Database": 0,
    "Timeout": 5000
  }
}
```

### Environment-Specific Configuration

```json
// appsettings.Development.json
{
  "Database": {
    "ConnectionString": "Server=localhost;Database=MyApp_Dev;Integrated Security=true;",
    "CommandTimeout": 300
  }
}

// appsettings.Production.json  
{
  "Database": {
    "ConnectionString": "Server=prod-server;Database=MyApp;User Id=app;Password=***;",
    "CommandTimeout": 30,
    "MaxPoolSize": 50
  }
}
```

## 🏗️ Key Features

### Enhanced Configuration Base

```csharp
public abstract class EnhancedConfiguration<T> : ConfigurationBase<T>, IValidatableObject
    where T : EnhancedConfiguration<T>, new()
{
    // IConfiguration binding support
    public virtual void Bind(IConfiguration configuration) { }
    
    // Options pattern integration
    public virtual void PostConfigure(IServiceProvider services) { }
    
    // Hot reload support
    public virtual void OnReload(T newConfiguration) { }
    
    // Custom validation beyond DataAnnotations
    public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        yield break; // Override in derived classes
    }
}
```

### Configuration Builder

```csharp
var config = new ConfigurationBuilder<DatabaseConfiguration>()
    .WithConnectionString("Server=localhost;Database=Test;")
    .WithTimeout(TimeSpan.FromSeconds(45))
    .WithMaxPoolSize(25)
    .EnableRetryOnFailure()
    .Build();

// Fluent validation
var result = config.ValidateAndBuild();
if (result.IsFailure)
{
    throw new InvalidOperationException(result.Error.Message);
}
```

### Configuration Sections

```csharp
// Nested configuration sections
public class ApplicationConfiguration : EnhancedConfiguration<ApplicationConfiguration>
{
    public DatabaseConfiguration Database { get; set; } = new();
    public RedisConfiguration Redis { get; set; } = new();
    public LoggingConfiguration Logging { get; set; } = new();
    
    public override bool IsValid => 
        Database.IsValid && 
        Redis.IsValid && 
        Logging.IsValid;
}
```

### Secret Management Integration

```csharp
public class SecureConfiguration : EnhancedConfiguration<SecureConfiguration>
{
    [Secret] // Custom attribute for sensitive data
    public string ApiKey { get; set; } = string.Empty;
    
    [Secret]
    public string DatabasePassword { get; set; } = string.Empty;
    
    // Secrets are automatically excluded from logging/serialization
    public override string ToString()
    {
        return $"SecureConfiguration {{ ApiKey: [HIDDEN], DatabasePassword: [HIDDEN] }}";
    }
}
```

## 🔧 Advanced Features

### Configuration Providers

```csharp
// Custom configuration provider
public class DatabaseConfigurationProvider : EnhancedConfigurationProvider<DatabaseConfiguration>
{
    public override async Task<DatabaseConfiguration> LoadAsync()
    {
        // Load from database, API, etc.
        var config = await _repository.GetConfigurationAsync("Database");
        return config ?? CreateDefault();
    }
    
    public override async Task SaveAsync(DatabaseConfiguration configuration)
    {
        await _repository.SaveConfigurationAsync("Database", configuration);
    }
}
```

### Configuration Monitoring

```csharp
public class ConfigurationMonitor : IHostedService
{
    private readonly IOptionsMonitor<DatabaseConfiguration> _monitor;
    
    public ConfigurationMonitor(IOptionsMonitor<DatabaseConfiguration> monitor)
    {
        _monitor = monitor;
        _monitor.OnChange(OnConfigurationChanged);
    }
    
    private void OnConfigurationChanged(DatabaseConfiguration config)
    {
        // React to configuration changes
        _logger.LogInformation("Database configuration changed: {Config}", config);
    }
}
```

### Configuration Validation

```csharp
public class DatabaseConfiguration : EnhancedConfiguration<DatabaseConfiguration>
{
    [Required]
    public string ConnectionString { get; set; } = string.Empty;
    
    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Custom business rule validation
        if (ConnectionString.Contains("localhost") && 
            validationContext.Items.ContainsKey("Environment") &&
            validationContext.Items["Environment"].ToString() == "Production")
        {
            yield return new ValidationResult(
                "Cannot use localhost in production environment",
                new[] { nameof(ConnectionString) });
        }
        
        // Validate connection string format
        if (!IsValidConnectionString(ConnectionString))
        {
            yield return new ValidationResult(
                "Invalid connection string format",
                new[] { nameof(ConnectionString) });
        }
    }
}
```

## 🧪 Testing

```csharp
[Test]
public void Configuration_WithValidValues_ShouldValidateSuccessfully()
{
    var config = new DatabaseConfiguration
    {
        ConnectionString = "Server=test;Database=test;",
        CommandTimeout = 30,
        MaxPoolSize = 10
    };
    
    config.IsValid.ShouldBeTrue();
    config.ValidationErrors.ShouldBeEmpty();
}

[Test]
public void Configuration_WithInvalidConnectionString_ShouldFailValidation()
{
    var config = new DatabaseConfiguration
    {
        ConnectionString = "", // Invalid
        CommandTimeout = 30
    };
    
    config.IsValid.ShouldBeFalse();
    config.ValidationErrors.ShouldContain("ConnectionString is required");
}
```

## 📚 Integration Examples

### ASP.NET Core

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Register all FractalDataWorks configurations
        services.AddFractalDataWorksConfiguration(Configuration);
        
        // Individual configuration registration
        services.Configure<DatabaseConfiguration>(Configuration.GetSection("Database"));
        services.Configure<RedisConfiguration>(Configuration.GetSection("Redis"));
        
        // Validation on startup
        services.AddSingleton<IStartupFilter, ConfigurationValidationStartupFilter>();
    }
}
```

### Console Applications

```csharp
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

var dbConfig = new DatabaseConfiguration();
configuration.GetSection("Database").Bind(dbConfig);

if (!dbConfig.IsValid)
{
    Console.WriteLine($"Configuration errors: {dbConfig.ValidationErrors}");
    return;
}
```

## 🔄 Version History

- **0.1.0-preview**: Initial release with IConfiguration integration
- **Future**: Secret management, configuration encryption, cloud provider integration

## 📄 License

Licensed under the Apache License 2.0. See [LICENSE](../../LICENSE) for details.