using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using Serilog;

namespace FractalDataWorks.Configuration.Extensions
{
    /// <summary>
    /// Extension methods for loading and validating configurations
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Binds a configuration section to a strongly-typed configuration object with validation
        /// </summary>
        public static T GetValidatedConfiguration<T>(
            this IConfiguration configuration, 
            string sectionName) 
            where T : ConfigurationBase<T>, new()
        {
            var config = new T();
            configuration.GetSection(sectionName).Bind(config);
            
            var validationResult = config.Validate();
            if (!validationResult.IsValid)
            {
                var errors = string.Join(Environment.NewLine, validationResult.Errors);
                throw new ConfigurationValidationException(
                    $"Configuration validation failed for {typeof(T).Name}:{Environment.NewLine}{errors}");
            }
            
            return config;
        }
        
        /// <summary>
        /// Binds a configuration section with a custom validator
        /// </summary>
        public static T GetValidatedConfiguration<T, TValidator>(
            this IConfiguration configuration, 
            string sectionName) 
            where T : class, new()
            where TValidator : IValidator<T>, new()
        {
            var config = new T();
            configuration.GetSection(sectionName).Bind(config);
            
            var validator = new TValidator();
            var validationResult = validator.Validate(config);
            
            if (!validationResult.IsValid)
            {
                var errors = string.Join(Environment.NewLine, 
                    validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ConfigurationValidationException(
                    $"Configuration validation failed for {typeof(T).Name}:{Environment.NewLine}{errors}");
            }
            
            return config;
        }
        
        /// <summary>
        /// Adds a validated configuration to the service collection
        /// </summary>
        public static IServiceCollection AddValidatedConfiguration<T>(
            this IServiceCollection services,
            IConfiguration configuration,
            string sectionName)
            where T : ConfigurationBase<T>, new()
        {
            var config = configuration.GetValidatedConfiguration<T>(sectionName);
            services.AddSingleton(config);
            services.AddSingleton<IConfigurationBase>(config);
            
            Log.Information("Registered configuration {ConfigType} from section {Section}", 
                typeof(T).Name, sectionName);
            
            return services;
        }
        
        /// <summary>
        /// Adds multiple validated configurations
        /// </summary>
        public static IServiceCollection AddValidatedConfigurations(
            this IServiceCollection services,
            IConfiguration configuration,
            params (Type configurationType, string sectionName)[] configurations)
        {
            foreach (var (configType, sectionName) in configurations)
            {
                var method = typeof(ConfigurationExtensions)
                    .GetMethod(nameof(AddValidatedConfiguration))
                    ?.MakeGenericMethod(configType);
                    
                method?.Invoke(null, new object[] { services, configuration, sectionName });
            }
            
            return services;
        }
        
        /// <summary>
        /// Validates all registered configurations
        /// </summary>
        public static IServiceCollection ValidateConfigurations(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            var configurations = serviceProvider.GetServices<IConfigurationBase>();
            
            var errors = new List<string>();
            foreach (var config in configurations)
            {
                var result = config.Validate();
                if (!result.IsValid)
                {
                    errors.Add($"{config.GetType().Name}: {string.Join(", ", result.Errors)}");
                }
            }
            
            if (errors.Any())
            {
                throw new ConfigurationValidationException(
                    $"Configuration validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
            }
            
            Log.Information("All {Count} configurations validated successfully", configurations.Count());
            return services;
        }
    }
}