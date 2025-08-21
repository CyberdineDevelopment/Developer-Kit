using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Samples.BasicService;

/// <summary>
/// Simple result wrapper demonstrating the FractalDataWorks result pattern.
/// </summary>
public class ServiceResult<T>
{
    public bool IsSuccess { get; private set; }
    public T? Value { get; private set; }
    public string? Message { get; private set; }

    private ServiceResult(bool isSuccess, T? value, string? message)
    {
        IsSuccess = isSuccess;
        Value = value;
        Message = message;
    }

    public static ServiceResult<T> Success(T value) => new(true, value, null);
    public static ServiceResult<T> Failure(string message) => new(false, default, message);
}

/// <summary>
/// Simple validation result demonstrating the FractalDataWorks validation pattern.
/// </summary>
public class ValidationResult
{
    public bool IsValid => !Errors.Any();
    public List<ValidationError> Errors { get; } = new();
}

/// <summary>
/// Simple validation error.
/// </summary>
public class ValidationError
{
    public string PropertyName { get; }
    public string ErrorMessage { get; }

    public ValidationError(string propertyName, string errorMessage)
    {
        PropertyName = propertyName;
        ErrorMessage = errorMessage;
    }
}

/// <summary>
/// Sample weather service that demonstrates the FractalDataWorks service pattern.
/// This is a simplified, standalone version that shows the concepts without framework dependencies.
/// </summary>
public sealed class WeatherService
{
    private readonly Random _random = new();
    private readonly ILogger<WeatherService> _logger;
    private readonly WeatherServiceConfiguration _configuration;
    
    public string Name => "Sample Weather Service";
    
    public WeatherService(
        ILogger<WeatherService> logger, 
        WeatherServiceConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Execute a weather command and return a typed result.
    /// </summary>
    public async Task<ServiceResult<T>> Execute<T>(WeatherCommand command, CancellationToken cancellationToken = default)
    {
        // Validate command
        var validationResult = command.Validate();
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            return ServiceResult<T>.Failure($"Command validation failed: {errors}");
        }

        // Log the command execution
        _logger.LogInformation("Fetching weather for {City}", command.City);
        
        // Simulate API call delay
        await Task.Delay(_configuration.SimulatedDelayMs, cancellationToken);
        
        // Check if the service configuration is valid
        if (!_configuration.IsValid)
        {
            return ServiceResult<T>.Failure("Weather service configuration is invalid");
        }
        
        // Simulate weather data
        var temperature = _random.Next(_configuration.MinTemperature, _configuration.MaxTemperature);
        var conditions = _configuration.PossibleConditions[_random.Next(_configuration.PossibleConditions.Length)];
        
        var result = new WeatherResult
        {
            City = command.City,
            Temperature = temperature,
            Conditions = conditions,
            Unit = _configuration.TemperatureUnit,
            Timestamp = DateTime.UtcNow
        };
        
        _logger.LogInformation("Weather for {City}: {Temperature}°{Unit}, {Conditions}", 
            result.City, result.Temperature, result.Unit, result.Conditions);
        
        // Handle different return types
        if (typeof(T) == typeof(WeatherResult))
        {
            return ServiceResult<T>.Success((T)(object)result);
        }
        
        if (typeof(T) == typeof(object))
        {
            return ServiceResult<T>.Success((T)(object)result);
        }
        
        return ServiceResult<T>.Failure($"Cannot convert WeatherResult to {typeof(T).Name}");
    }

    /// <summary>
    /// Execute a weather command without typed result.
    /// </summary>
    public async Task<ServiceResult<WeatherResult>> Execute(WeatherCommand command, CancellationToken cancellationToken = default)
    {
        return await Execute<WeatherResult>(command, cancellationToken);
    }
}

/// <summary>
/// Command to get weather information for a city.
/// Demonstrates the FractalDataWorks command pattern.
/// </summary>
public sealed class WeatherCommand
{
    /// <summary>
    /// Gets or sets the city to get weather for.
    /// </summary>
    public string City { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets whether to include forecast data.
    /// </summary>
    public bool IncludeForecast { get; set; }

    /// <summary>
    /// Unique identifier for this command.
    /// </summary>
    public Guid CommandId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Correlation ID for tracking across operations.
    /// </summary>
    public Guid CorrelationId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// When this command was created.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Validate this command.
    /// </summary>
    public ValidationResult Validate()
    {
        var result = new ValidationResult();
        
        if (string.IsNullOrWhiteSpace(City))
        {
            result.Errors.Add(new ValidationError(nameof(City), "City is required"));
        }
        
        return result;
    }
}

/// <summary>
/// Weather service configuration with validation.
/// Demonstrates the FractalDataWorks configuration pattern.
/// </summary>
public sealed class WeatherServiceConfiguration
{
    /// <summary>
    /// Gets or sets the simulated API delay in milliseconds.
    /// </summary>
    public int SimulatedDelayMs { get; set; } = 500;
    
    /// <summary>
    /// Gets or sets the minimum temperature for simulated data.
    /// </summary>
    public int MinTemperature { get; set; } = -10;
    
    /// <summary>
    /// Gets or sets the maximum temperature for simulated data.
    /// </summary>
    public int MaxTemperature { get; set; } = 40;
    
    /// <summary>
    /// Gets or sets the temperature unit (C or F).
    /// </summary>
    public string TemperatureUnit { get; set; } = "C";
    
    /// <summary>
    /// Gets or sets the possible weather conditions.
    /// </summary>
    public string[] PossibleConditions { get; set; } = 
    {
        "Sunny", "Cloudy", "Rainy", "Snowy", "Windy", "Foggy", "Stormy"
    };

    /// <summary>
    /// Configuration section name for binding.
    /// </summary>
    public string SectionName => "WeatherService";

    /// <summary>
    /// Validate this configuration.
    /// </summary>
    public ValidationResult Validate()
    {
        var result = new ValidationResult();
        
        if (SimulatedDelayMs < 0)
        {
            result.Errors.Add(new ValidationError(nameof(SimulatedDelayMs), "Simulated delay cannot be negative"));
        }
        
        if (MinTemperature >= MaxTemperature)
        {
            result.Errors.Add(new ValidationError(nameof(MinTemperature), "Minimum temperature must be less than maximum temperature"));
        }
        
        if (string.IsNullOrWhiteSpace(TemperatureUnit))
        {
            result.Errors.Add(new ValidationError(nameof(TemperatureUnit), "Temperature unit is required"));
        }
        else if (TemperatureUnit != "C" && TemperatureUnit != "F")
        {
            result.Errors.Add(new ValidationError(nameof(TemperatureUnit), "Temperature unit must be 'C' or 'F'"));
        }
        
        if (PossibleConditions == null || PossibleConditions.Length == 0)
        {
            result.Errors.Add(new ValidationError(nameof(PossibleConditions), "At least one weather condition is required"));
        }
        
        return result;
    }

    /// <summary>
    /// Gets a value indicating whether this configuration is valid.
    /// </summary>
    public bool IsValid => Validate().IsValid;
}

/// <summary>
/// Result from the weather service.
/// </summary>
public sealed class WeatherResult
{
    public string City { get; set; } = string.Empty;
    public int Temperature { get; set; }
    public string Conditions { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    
    public override string ToString()
    {
        return $"{City}: {Temperature}°{Unit}, {Conditions} at {Timestamp:yyyy-MM-dd HH:mm:ss} UTC";
    }
}