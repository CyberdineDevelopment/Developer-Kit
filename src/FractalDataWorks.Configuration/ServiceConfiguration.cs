using System;
using System.Collections.Generic;
using FluentValidation;

namespace FractalDataWorks.Configuration
{
    /// <summary>
    /// Base configuration for services
    /// </summary>
    public abstract class ServiceConfiguration : ConfigurationBase
    {
        /// <summary>
        /// The service name
        /// </summary>
        public string ServiceName { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether the service is enabled
        /// </summary>
        public bool Enabled { get; set; } = true;
        
        /// <summary>
        /// Service timeout
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
        
        /// <summary>
        /// Number of retry attempts
        /// </summary>
        public int RetryCount { get; set; } = 3;
        
        /// <summary>
        /// Whether to enable logging for this service
        /// </summary>
        public bool EnableLogging { get; set; } = true;
        
        /// <summary>
        /// Additional service-specific settings
        /// </summary>
        public Dictionary<string, object> Settings { get; set; } = new();
        
        protected override IValidator<ServiceConfiguration>? CreateValidator()
            => new ServiceConfigurationValidator();
    }
    
    /// <summary>
    /// Generic service configuration
    /// </summary>
    /// <typeparam name="T">The concrete configuration type</typeparam>
    public abstract class ServiceConfiguration<T> : ServiceConfiguration
        where T : ServiceConfiguration<T>, new()
    {
    }
    
    /// <summary>
    /// Validator for service configuration
    /// </summary>
    public class ServiceConfigurationValidator : AbstractValidator<ServiceConfiguration>
    {
        public ServiceConfigurationValidator()
        {
            RuleFor(x => x.ServiceName)
                .NotEmpty()
                .WithMessage("Service name is required")
                .MaximumLength(100)
                .WithMessage("Service name cannot exceed 100 characters");
                
            RuleFor(x => x.Timeout)
                .GreaterThan(TimeSpan.Zero)
                .WithMessage("Timeout must be greater than zero")
                .LessThanOrEqualTo(TimeSpan.FromMinutes(30))
                .WithMessage("Timeout cannot exceed 30 minutes");
                
            RuleFor(x => x.RetryCount)
                .InclusiveBetween(0, 10)
                .WithMessage("Retry count must be between 0 and 10");
        }
    }
}