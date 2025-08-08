using System;
using System.IO;
using System.Threading.Tasks;
using FractalDataWorks;
using FractalDataWorks.Configuration;
using FractalDataWorks.Configuration.Sources;
using FractalDataWorks.Samples.BasicService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var services = new ServiceCollection();

// Add logging
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

// Build service provider
var serviceProvider = services.BuildServiceProvider();
var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

// Create configuration directory
var configPath = Path.Combine(Directory.GetCurrentDirectory(), "configs");
Directory.CreateDirectory(configPath);

// Create JSON configuration source
var jsonSource = new JsonConfigurationSource(
    loggerFactory.CreateLogger<JsonConfigurationSource>(), 
    configPath);

// Create a sample configuration
var config = new WeatherServiceConfiguration
{
    Id = 1,
    Name = "Default Weather Service",
    IsEnabled = true,
    SimulatedDelayMs = 200,
    MinTemperature = -5,
    MaxTemperature = 35,
    TemperatureUnit = "C",
    PossibleConditions = new[] { "Sunny", "Cloudy", "Rainy", "Windy" }
};

// Validate the configuration
Console.WriteLine("=== Configuration Validation ===");
var validationResult = await config.Validate();
if (validationResult.IsValid)
{
    Console.WriteLine("✓ Configuration is valid");
}
else
{
    Console.WriteLine("✗ Configuration is invalid:");
    foreach (var error in validationResult.Errors)
    {
        Console.WriteLine($"  - {error.PropertyName}: {error.ErrorMessage}");
    }
    return;
}

// Save configuration to JSON
Console.WriteLine("\n=== Saving Configuration ===");
var saveResult = await jsonSource.Save(config);
if (saveResult.IsSuccess)
{
    Console.WriteLine($"✓ Configuration saved to {Path.Combine(configPath, $"WeatherServiceConfiguration_{config.Id}.json")}");
}
else
{
    Console.WriteLine($"✗ Failed to save configuration: {saveResult.Message}");
}

// Load configuration from JSON
Console.WriteLine("\n=== Loading Configuration ===");
var loadResult = await jsonSource.Load<WeatherServiceConfiguration>(1);
if (loadResult.IsSuccess)
{
    Console.WriteLine($"✓ Configuration loaded: {loadResult.Value.Name}");
    config = loadResult.Value;
}
else
{
    Console.WriteLine($"✗ Failed to load configuration: {loadResult.Message}");
}

// Create the weather service
Console.WriteLine("\n=== Creating Weather Service ===");
var weatherService = new WeatherService(
    loggerFactory.CreateLogger<WeatherService>(),
    config);

Console.WriteLine($"✓ Service created: {weatherService.Name}");
Console.WriteLine($"  ID: {weatherService.Id}");
Console.WriteLine($"  Type: {weatherService.ServiceType}");
Console.WriteLine($"  Available: {weatherService.IsAvailable}");

// Execute some weather commands
Console.WriteLine("\n=== Executing Weather Commands ===");

var cities = new[] { "New York", "London", "Tokyo", "Sydney", "Berlin" };

foreach (var city in cities)
{
    var command = new WeatherCommand
    {
        City = city,
        IncludeForecast = false
    };
    
    Console.WriteLine($"\nFetching weather for {city}...");
    var result = await weatherService.Execute(command);
    
    if (result.IsSuccess && result is IFdwResult<WeatherResult> weatherResult)
    {
        Console.WriteLine($"✓ {weatherResult.Value}");
    }
    else
    {
        Console.WriteLine($"✗ Failed: {result.Message}");
    }
}

// Demonstrate disabling the service
Console.WriteLine("\n=== Testing Disabled Service ===");
config.IsEnabled = false;
await jsonSource.Save(config);

var disabledService = new WeatherService(
    loggerFactory.CreateLogger<WeatherService>(),
    config);

var testCommand = new WeatherCommand { City = "Test City" };
var disabledResult = await disabledService.Execute(testCommand);

if (!disabledResult.IsSuccess)
{
    Console.WriteLine($"✓ Service correctly returned error when disabled: {disabledResult.Message}");
}

// Clean up - delete the configuration
Console.WriteLine("\n=== Cleanup ===");
var deleteResult = await jsonSource.Delete<WeatherServiceConfiguration>(1);
if (deleteResult.IsSuccess)
{
    Console.WriteLine("✓ Configuration deleted");
}

Console.WriteLine("\n=== Sample Complete ===");
Console.WriteLine("This sample demonstrated:");
Console.WriteLine("- Creating and validating configurations");
Console.WriteLine("- Saving/loading configurations with JSON");
Console.WriteLine("- Creating services with configurations");
Console.WriteLine("- Executing commands and handling results");
Console.WriteLine("- Service lifecycle management");
