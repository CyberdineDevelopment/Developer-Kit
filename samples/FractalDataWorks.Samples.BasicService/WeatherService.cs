using System;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks;
using FractalDataWorks.Configuration;
using FractalDataWorks.Services;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Samples.BasicService;

/// <summary>
/// Sample weather service that demonstrates the FractalDataWorks service pattern.
/// </summary>
public class WeatherService : ServiceBase<WeatherCommand, WeatherServiceConfiguration, WeatherService>
{
    private readonly Random _random = new();
    
    public WeatherService(
        ILogger<WeatherService>? logger, 
        WeatherServiceConfiguration configuration) 
        : base(logger, configuration)
    {
    }

    protected override async Task<IFdwResult> ExecuteCore(
        WeatherCommand command, 
        CancellationToken cancellationToken)
    {
        // Log the command execution
        Logger.LogInformation("Fetching weather for {City}", command.City);
        
        // Simulate API call delay
        await Task.Delay(Configuration.SimulatedDelayMs, cancellationToken);
        
        // Check if the service is enabled
        if (!Configuration.IsEnabled)
        {
            return FdwResult.Failure("Weather service is disabled");
        }
        
        // Simulate weather data
        var temperature = _random.Next(Configuration.MinTemperature, Configuration.MaxTemperature);
        var conditions = Configuration.PossibleConditions[_random.Next(Configuration.PossibleConditions.Length)];
        
        var result = new WeatherResult
        {
            City = command.City,
            Temperature = temperature,
            Conditions = conditions,
            Unit = Configuration.TemperatureUnit,
            Timestamp = DateTime.UtcNow
        };
        
        Logger.LogInformation("Weather for {City}: {Temperature}°{Unit}, {Conditions}", 
            result.City, result.Temperature, result.Unit, result.Conditions);
        
        return FdwResult<WeatherResult>.Success(result);
    }

    protected override async Task<IFdwResult<TOut>> ExecuteCore<TOut>(
        WeatherCommand command, 
        CancellationToken cancellationToken)
    {
        var result = await ExecuteCore(command, cancellationToken);
        
        if (!result.IsSuccess)
        {
            return FdwResult<TOut>.Failure(result.Message);
        }
        
        if (result is IFdwResult<WeatherResult> weatherResult && weatherResult.Value is TOut typedResult)
        {
            return FdwResult<TOut>.Success(typedResult);
        }
        
        return FdwResult<TOut>.Failure("Invalid result type");
    }
}

/// <summary>
/// Command to get weather information for a city.
/// </summary>
public class WeatherCommand : ICommand
{
    /// <summary>
    /// Gets or sets the city to get weather for.
    /// </summary>
    public string City { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets whether to include forecast data.
    /// </summary>
    public bool IncludeForecast { get; set; }
}

/// <summary>
/// Weather service configuration with validation.
/// </summary>
public class WeatherServiceConfiguration : ConfigurationBase<WeatherServiceConfiguration>
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
    
    protected override IValidator<WeatherServiceConfiguration>? CreateValidator()
    {
        return new WeatherServiceConfigurationValidator();
    }
}

/// <summary>
/// Result from the weather service.
/// </summary>
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