using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Samples.BasicService;

/// <summary>
/// Sample application demonstrating the FractalDataWorks service patterns.
/// This is a simplified, standalone version that shows the core concepts
/// without requiring the full FractalDataWorks framework dependencies.
/// </summary>
internal static class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine("FractalDataWorks Basic Service Sample");
        Console.WriteLine("=====================================");
        Console.WriteLine("This sample demonstrates key FractalDataWorks patterns:");
        Console.WriteLine("- Service configuration with validation");
        Console.WriteLine("- Command pattern with validation");
        Console.WriteLine("- Result pattern for error handling");
        Console.WriteLine("- Dependency injection and logging");
        Console.WriteLine();

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
        var logger = serviceProvider.GetRequiredService<ILogger<WeatherService>>();

        // Create a sample configuration
        var config = new WeatherServiceConfiguration
        {
            SimulatedDelayMs = 200,
            MinTemperature = -5,
            MaxTemperature = 35,
            TemperatureUnit = "C",
            PossibleConditions = new[] { "Sunny", "Cloudy", "Rainy", "Windy" }
        };

        // Validate the configuration
        Console.WriteLine("=== Configuration Validation ===");
        var validationResult = config.Validate();
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

        // Create the weather service
        Console.WriteLine("\n=== Creating Weather Service ===");
        var weatherService = new WeatherService(logger, config);

        Console.WriteLine($"✓ Service created: {weatherService.Name}");

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
            var result = await weatherService.Execute<WeatherResult>(command, default);
            
            if (result.IsSuccess)
            {
                Console.WriteLine($"✓ {result.Value}");
            }
            else
            {
                Console.WriteLine($"✗ Failed: {result.Message}");
            }
        }

        // Demonstrate invalid configuration
        Console.WriteLine("\n=== Testing Invalid Configuration ===");
        var invalidConfig = new WeatherServiceConfiguration
        {
            MinTemperature = 50,
            MaxTemperature = 10, // Invalid: min > max
            TemperatureUnit = "X" // Invalid unit
        };
        
        var invalidService = new WeatherService(logger, invalidConfig);
        var testCommand = new WeatherCommand { City = "Test City" };
        var invalidResult = await invalidService.Execute(testCommand, default);

        if (!invalidResult.IsSuccess)
        {
            Console.WriteLine($"✓ Service correctly rejected invalid configuration: {invalidResult.Message}");
        }

        // Demonstrate command validation
        Console.WriteLine("\n=== Testing Command Validation ===");
        var emptyCommand = new WeatherCommand { City = "" }; // Invalid: empty city
        var commandResult = await weatherService.Execute(emptyCommand, default);
        
        if (!commandResult.IsSuccess)
        {
            Console.WriteLine($"✓ Service correctly rejected invalid command: {commandResult.Message}");
        }

        Console.WriteLine("\n=== Sample Complete ===");
        Console.WriteLine("This sample demonstrated:");
        Console.WriteLine("- Creating and validating configurations using the FractalDataWorks pattern");
        Console.WriteLine("- Creating services with dependency injection and logging");
        Console.WriteLine("- Executing commands using the command pattern");
        Console.WriteLine("- Handling results using the result pattern");
        Console.WriteLine("- Configuration and command validation");
        Console.WriteLine();
        Console.WriteLine("Key FractalDataWorks concepts shown:");
        Console.WriteLine("- ServiceResult<T> pattern for consistent error handling");
        Console.WriteLine("- Configuration validation with ValidationResult");
        Console.WriteLine("- Command pattern with built-in validation");
        Console.WriteLine("- Structured logging with correlation IDs");
        Console.WriteLine();
        Console.WriteLine("To learn more about the full FractalDataWorks framework,");
        Console.WriteLine("see the documentation and other samples.");
    }
}
