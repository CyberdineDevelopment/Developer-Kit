using FluentValidation;

namespace FractalDataWorks.Samples.BasicService;

/// <summary>
/// Validator for WeatherServiceConfiguration.
/// </summary>
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
            .Must(x => x.Length > 0)
            .WithMessage("Possible conditions array cannot be empty");
    }
}