using System;
using System.Collections.Generic;
using FluentValidation;
using Serilog;
using Serilog.Events;
using Serilog.Configuration;

namespace FractalDataWorks.Configuration.Logging
{
    /// <summary>
    /// Configuration for Serilog logging
    /// </summary>
    public class SerilogConfiguration : ConfigurationBase<SerilogConfiguration>
    {
        /// <summary>
        /// Minimum log level
        /// </summary>
        public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Information;
        
        /// <summary>
        /// Whether to enrich logs with thread information
        /// </summary>
        public bool EnrichWithThreadInfo { get; set; } = true;
        
        /// <summary>
        /// Whether to enrich logs with process information
        /// </summary>
        public bool EnrichWithProcessInfo { get; set; } = true;
        
        /// <summary>
        /// Whether to enrich logs with environment information
        /// </summary>
        public bool EnrichWithEnvironmentInfo { get; set; } = true;
        
        /// <summary>
        /// Whether to enrich logs with memory information
        /// </summary>
        public bool EnrichWithMemoryInfo { get; set; } = false;
        
        /// <summary>
        /// Console output configuration
        /// </summary>
        public ConsoleOutputConfiguration Console { get; set; } = new();
        
        /// <summary>
        /// File output configuration
        /// </summary>
        public FileOutputConfiguration? File { get; set; }
        
        /// <summary>
        /// Custom properties to add to all log events
        /// </summary>
        public Dictionary<string, string> Properties { get; set; } = new();
        
        /// <summary>
        /// Override log levels for specific namespaces
        /// </summary>
        public Dictionary<string, LogEventLevel> Overrides { get; set; } = new();
        
        protected override IValidator<SerilogConfiguration>? CreateValidator()
            => new SerilogConfigurationValidator();
    }
    
    /// <summary>
    /// Console output configuration
    /// </summary>
    public class ConsoleOutputConfiguration
    {
        /// <summary>
        /// Whether console output is enabled
        /// </summary>
        public bool Enabled { get; set; } = true;
        
        /// <summary>
        /// Output template
        /// </summary>
        public string OutputTemplate { get; set; } = 
            "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";
            
        /// <summary>
        /// Whether to use colored output
        /// </summary>
        public bool UseColors { get; set; } = true;
    }
    
    /// <summary>
    /// File output configuration
    /// </summary>
    public class FileOutputConfiguration
    {
        /// <summary>
        /// Whether file output is enabled
        /// </summary>
        public bool Enabled { get; set; } = true;
        
        /// <summary>
        /// Path to the log file
        /// </summary>
        public string Path { get; set; } = "logs/FractalDataWorks-.log";
        
        /// <summary>
        /// Rolling interval
        /// </summary>
        public RollingInterval RollingInterval { get; set; } = RollingInterval.Day;
        
        /// <summary>
        /// Maximum file size in bytes (null for unlimited)
        /// </summary>
        public long? FileSizeLimitBytes { get; set; } = 1073741824; // 1GB
        
        /// <summary>
        /// Number of files to retain (null for unlimited)
        /// </summary>
        public int? RetainedFileCountLimit { get; set; } = 31;
        
        /// <summary>
        /// Output template
        /// </summary>
        public string OutputTemplate { get; set; } = 
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
    }
    
    /// <summary>
    /// Validator for Serilog configuration
    /// </summary>
    public class SerilogConfigurationValidator : AbstractValidator<SerilogConfiguration>
    {
        public SerilogConfigurationValidator()
        {
            RuleFor(x => x.MinimumLevel)
                .IsInEnum()
                .WithMessage("Invalid log level");
                
            When(x => x.File != null && x.File.Enabled, () =>
            {
                RuleFor(x => x.File!.Path)
                    .NotEmpty()
                    .WithMessage("File path is required when file logging is enabled");
                    
                RuleFor(x => x.File!.RetainedFileCountLimit)
                    .GreaterThan(0)
                    .When(x => x.File!.RetainedFileCountLimit.HasValue)
                    .WithMessage("Retained file count must be greater than 0");
                    
                RuleFor(x => x.File!.FileSizeLimitBytes)
                    .GreaterThan(1024) // At least 1KB
                    .When(x => x.File!.FileSizeLimitBytes.HasValue)
                    .WithMessage("File size limit must be at least 1KB");
            });
            
            RuleForEach(x => x.Overrides)
                .Must(x => Enum.IsDefined(typeof(LogEventLevel), x.Value))
                .WithMessage("Invalid log level in overrides");
        }
    }
}