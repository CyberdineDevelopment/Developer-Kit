using System;
using System.IO;
using System.Linq;
using FluentValidation;

namespace FractalDataWorks.Configuration.Validators
{
    /// <summary>
    /// Common validation rules for configuration objects
    /// </summary>
    public static class CommonValidators
    {
        /// <summary>
        /// Validates that a string is a valid file path
        /// </summary>
        public static IRuleBuilderOptions<T, string> MustBeValidPath<T>(
            this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .Must(path => !string.IsNullOrWhiteSpace(path) && IsValidPath(path))
                .WithMessage("{PropertyName} must be a valid file path");
        }
        
        /// <summary>
        /// Validates that a directory exists
        /// </summary>
        public static IRuleBuilderOptions<T, string> DirectoryMustExist<T>(
            this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .Must(path => !string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
                .WithMessage("{PropertyName} directory does not exist: '{PropertyValue}'");
        }
        
        /// <summary>
        /// Validates that a file exists
        /// </summary>
        public static IRuleBuilderOptions<T, string> FileMustExist<T>(
            this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .Must(path => !string.IsNullOrWhiteSpace(path) && File.Exists(path))
                .WithMessage("{PropertyName} file does not exist: '{PropertyValue}'");
        }
        
        /// <summary>
        /// Validates a connection string
        /// </summary>
        public static IRuleBuilderOptions<T, string> MustBeValidConnectionString<T>(
            this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("{PropertyName} cannot be empty")
                .Must(IsValidConnectionString)
                .WithMessage("{PropertyName} is not a valid connection string");
        }
        
        /// <summary>
        /// Validates a URL
        /// </summary>
        public static IRuleBuilderOptions<T, string> MustBeValidUrl<T>(
            this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri) && 
                     (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                .WithMessage("{PropertyName} must be a valid HTTP or HTTPS URL");
        }
        
        /// <summary>
        /// Validates a port number
        /// </summary>
        public static IRuleBuilderOptions<T, int> MustBeValidPort<T>(
            this IRuleBuilder<T, int> ruleBuilder)
        {
            return ruleBuilder
                .InclusiveBetween(1, 65535)
                .WithMessage("{PropertyName} must be between 1 and 65535");
        }
        
        /// <summary>
        /// Validates a timeout value
        /// </summary>
        public static IRuleBuilderOptions<T, int> MustBeValidTimeout<T>(
            this IRuleBuilder<T, int> ruleBuilder)
        {
            return ruleBuilder
                .GreaterThan(0)
                .LessThanOrEqualTo(300000) // 5 minutes
                .WithMessage("{PropertyName} must be between 1 and 300000 milliseconds");
        }
        
        /// <summary>
        /// Validates an enum value
        /// </summary>
        public static IRuleBuilderOptions<T, TEnum> MustBeValidEnum<T, TEnum>(
            this IRuleBuilder<T, TEnum> ruleBuilder) 
            where TEnum : struct, Enum
        {
            return ruleBuilder
                .Must(value => Enum.IsDefined(typeof(TEnum), value))
                .WithMessage("{PropertyName} must be a valid {PropertyType} value");
        }
        
        private static bool IsValidPath(string path)
        {
            try
            {
                var fullPath = Path.GetFullPath(path);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        private static bool IsValidConnectionString(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                return false;
                
            // Basic validation - has key=value pairs
            var pairs = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);
            return pairs.All(pair => pair.Contains('='));
        }
    }
}