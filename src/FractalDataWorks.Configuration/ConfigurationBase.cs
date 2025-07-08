using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.Configuration;
using FluentValidation;
using FluentValidation.Results;

namespace FractalDataWorks.Configuration
{
    /// <summary>
    /// Base class for all configurations in the FractalDataWorks ecosystem.
    /// Provides self-validation using FluentValidation.
    /// </summary>
    public abstract class ConfigurationBase : IConfigurationBase
    {
        /// <summary>
        /// Validates this configuration using the associated validator
        /// </summary>
        /// <returns>Validation results</returns>
        public abstract IValidationResult Validate();
        
        /// <summary>
        /// Gets the validator for this configuration type
        /// </summary>
        /// <returns>The validator instance or null if no validator is defined</returns>
        protected abstract IValidator? GetValidator();
    }
    
    /// <summary>
    /// Generic base class for strongly-typed configurations with FluentValidation support
    /// </summary>
    /// <typeparam name="T">The concrete configuration type</typeparam>
    public abstract class ConfigurationBase<T> : ConfigurationBase where T : ConfigurationBase<T>, new()
    {
        private IValidator<T>? _validator;
        
        /// <summary>
        /// Gets or sets the validator for this configuration type
        /// </summary>
        protected IValidator<T>? Validator
        {
            get => _validator ??= CreateValidator();
            set => _validator = value;
        }
        
        /// <summary>
        /// Creates the validator for this configuration type
        /// Override this to provide custom validation rules
        /// </summary>
        protected virtual IValidator<T>? CreateValidator() => null;
        
        /// <summary>
        /// Gets the validator for this configuration
        /// </summary>
        protected override IValidator? GetValidator() => Validator;
        
        /// <summary>
        /// Validates this configuration
        /// </summary>
        public override IValidationResult Validate()
        {
            if (Validator is null)
            {
                return ValidationResult.Success();
            }
            
            var result = Validator.Validate((T)this);
            return new ValidationResult
            {
                IsValid = result.IsValid,
                Errors = result.Errors.Select(e => e.ErrorMessage).ToArray()
            };
        }
        
        /// <summary>
        /// Creates a default instance of this configuration type
        /// </summary>
        /// <returns>A default instance</returns>
        public virtual T CreateDefault() => new();
        
        /// <summary>
        /// Creates an invalid instance of this configuration type
        /// </summary>
        /// <returns>An invalid instance for error handling</returns>
        public virtual T CreateInvalid() => CreateDefault();
    }
    
    /// <summary>
    /// Validation result implementation
    /// </summary>
    public record ValidationResult : IValidationResult
    {
        /// <summary>
        /// Whether the validation passed
        /// </summary>
        public bool IsValid { get; init; }
        
        /// <summary>
        /// Validation error messages
        /// </summary>
        public string[] Errors { get; init; } = Array.Empty<string>();
        
        /// <summary>
        /// Creates a successful validation result
        /// </summary>
        public static ValidationResult Success() => new() { IsValid = true };
        
        /// <summary>
        /// Creates a failed validation result
        /// </summary>
        public static ValidationResult Failure(params string[] errors) => new() 
        { 
            IsValid = false, 
            Errors = errors ?? Array.Empty<string>() 
        };
        
        /// <summary>
        /// Creates a validation result from FluentValidation result
        /// </summary>
        public static ValidationResult From(FluentValidation.Results.ValidationResult result) => new()
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(e => e.ErrorMessage).ToArray()
        };
        
        /// <summary>
        /// Gets the first error message or empty string
        /// </summary>
        public string FirstError => Errors.Length > 0 ? Errors[0] : string.Empty;
        
        /// <summary>
        /// Gets all errors as a single string
        /// </summary>
        public string AllErrors => string.Join("; ", Errors);
    }
}