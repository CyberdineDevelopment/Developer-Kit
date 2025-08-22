using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Samples.BasicService;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== FractalDataWorks BasicService Sample ===");
        Console.WriteLine("This sample demonstrates basic patterns used with FractalDataWorks:");
        Console.WriteLine("- Service pattern with dependency injection");
        Console.WriteLine("- Configuration validation with FluentValidation");
        Console.WriteLine("- Command pattern with validation");
        Console.WriteLine("- Structured logging");
        
        // Setup dependency injection
        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        var serviceProvider = services.BuildServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

        // Create configuration
        var config = new WeatherServiceConfiguration
        {
            IsEnabled = true,
            SimulatedDelayMs = 200,
            MinTemperature = -5,
            MaxTemperature = 35,
            TemperatureUnit = "C",
            PossibleConditions = new[] { "Sunny", "Cloudy", "Rainy", "Windy", "Stormy" }
        };

        // Validate configuration
        Console.WriteLine("\n=== Configuration Validation ===");
        var configValidation = config.Validate();
        if (configValidation.IsValid)
        {
            Console.WriteLine("✓ Configuration is valid");
        }
        else
        {
            Console.WriteLine("✗ Configuration is invalid:");
            foreach (var error in configValidation.Errors)
            {
                Console.WriteLine($"  - {error.PropertyName}: {error.ErrorMessage}");
            }
            return;
        }

        // Create service
        Console.WriteLine("\n=== Creating Weather Service ===");
        var weatherService = new WeatherService(
            loggerFactory.CreateLogger<WeatherService>(),
            config);

        Console.WriteLine("✓ Service created successfully");

        // Execute commands
        Console.WriteLine("\n=== Executing Weather Commands ===");
        var cities = new[] { "New York", "London", "Tokyo", "Sydney", "Berlin" };

        foreach (var city in cities)
        {
            var command = new WeatherCommand { City = city };
            Console.WriteLine($"\nFetching weather for {city}...");
            
            try
            {
                var result = await weatherService.GetWeatherAsync(command);
                if (result != null)
                {
                    Console.WriteLine($"✓ {result}");
                }
                else
                {
                    Console.WriteLine("✗ No weather data returned");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Exception: {ex.Message}");
            }
        }

        // Demonstrate command validation
        Console.WriteLine("\n=== Testing Command Validation ===");
        var invalidCommand = new WeatherCommand { City = "" }; // Empty city should fail
        Console.WriteLine("Testing with empty city name...");
        
        var invalidResult = await weatherService.GetWeatherAsync(invalidCommand);
        if (invalidResult == null)
        {
            Console.WriteLine("✓ Validation correctly rejected empty city name");
        }

        // Demonstrate disabled service
        Console.WriteLine("\n=== Testing Disabled Service ===");
        config.IsEnabled = false;
        var disabledCommand = new WeatherCommand { City = "Test City" };
        var disabledResult = await weatherService.GetWeatherAsync(disabledCommand);
        
        if (disabledResult == null)
        {
            Console.WriteLine("✓ Service correctly returned null when disabled");
        }

        Console.WriteLine("\n=== Sample Complete ===");
        Console.WriteLine("This sample showed:");
        Console.WriteLine("- Dependency injection with Microsoft.Extensions");
        Console.WriteLine("- FluentValidation for configuration and command validation");
        Console.WriteLine("- Structured logging with Microsoft.Extensions.Logging");
        Console.WriteLine("- Service pattern that could be extended with FractalDataWorks");
        Console.WriteLine("- Command pattern with validation");
    }
}
