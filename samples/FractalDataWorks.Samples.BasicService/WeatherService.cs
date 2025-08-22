using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FluentValidation;
using FluentValidation.Results;

namespace FractalDataWorks.Samples.BasicService;

// This sample demonstrates basic patterns that would be used with FractalDataWorks
// In a real implementation, these would inherit from FractalDataWorks base classes

public class WeatherService
{
    private readonly ILogger<WeatherService> _logger;
    private readonly WeatherServiceConfiguration _configuration;
    private readonly Random _random = new();
    
    public WeatherService(ILogger<WeatherService> logger, WeatherServiceConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<WeatherResult?> GetWeatherAsync(WeatherCommand command)
    {
        _logger.LogInformation("Fetching weather for {City}", command.City);
        
        // Validate the command
        var validator = new WeatherCommandValidator();
        var validationResult = validator.Validate(command);
        
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Invalid command: {Errors}", string.Join(", ", validationResult.Errors));
            return null;
        }
        
        // Simulate API call delay
        await Task.Delay(_configuration.SimulatedDelayMs);
        
        if (!_configuration.IsEnabled)
        {
            _logger.LogWarning("Weather service is disabled");
            return null;
        }
        
        // Generate simulated weather data
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
        
        return result;
    }
}

public class WeatherCommand
{
    public string City { get; set; } = string.Empty;
    public bool IncludeForecast { get; set; }
}

public class WeatherCommandValidator : AbstractValidator<WeatherCommand>
{
    public WeatherCommandValidator()
    {
        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("City cannot be empty")
            .MaximumLength(100)
            .WithMessage("City name cannot exceed 100 characters");
    }
}

public class WeatherServiceConfiguration
{
    public bool IsEnabled { get; set; } = true;
    public int SimulatedDelayMs { get; set; } = 500;
    public int MinTemperature { get; set; } = -10;
    public int MaxTemperature { get; set; } = 40;
    public string TemperatureUnit { get; set; } = "C";
    public string[] PossibleConditions { get; set; } = 
    {
        "Sunny", "Cloudy", "Rainy", "Snowy", "Windy", "Foggy", "Stormy"
    };
    
    public ValidationResult Validate()
    {
        var validator = new WeatherServiceConfigurationValidator();
        return validator.Validate(this);
    }
}

public class WeatherServiceConfigurationValidator : AbstractValidator<WeatherServiceConfiguration>
{
    public WeatherServiceConfigurationValidator()
    {
        RuleFor(x => x.SimulatedDelayMs)
            .InclusiveBetween(0, 10000)
            .WithMessage("Simulated delay must be between 0 and 10000 ms");
        
        RuleFor(x => x.MinTemperature)
            .LessThan(x => x.MaxTemperature)
            .WithMessage("Minimum temperature must be less than maximum temperature");
        
        RuleFor(x => x.MaxTemperature)
            .GreaterThan(x => x.MinTemperature)
            .WithMessage("Maximum temperature must be greater than minimum temperature");
        
        RuleFor(x => x.TemperatureUnit)
            .NotEmpty()
            .Must(x => x == "C" || x == "F")
            .WithMessage("Temperature unit must be 'C' or 'F'");
        
        RuleFor(x => x.PossibleConditions)
            .NotEmpty()
            .WithMessage("At least one weather condition must be defined")
            .Must(x => x != null && x.Length > 0)
            .WithMessage("Possible conditions array cannot be empty");
    }
}

public class WeatherResult
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
