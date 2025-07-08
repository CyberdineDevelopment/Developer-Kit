using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace FractalDataWorks.Configuration.Logging
{
    /// <summary>
    /// Extension methods for configuring Serilog
    /// </summary>
    public static class SerilogExtensions
    {
        /// <summary>
        /// Configures Serilog using SerilogConfiguration
        /// </summary>
        public static IHostBuilder UseDomesticatedSerilog(
            this IHostBuilder hostBuilder,
            string configSection = "Serilog")
        {
            return hostBuilder.UseSerilog((context, services, loggerConfiguration) =>
            {
                var config = context.Configuration
                    .GetValidatedConfiguration<SerilogConfiguration>(configSection);
                    
                ConfigureSerilog(loggerConfiguration, config, context.Configuration);
            });
        }
        
        /// <summary>
        /// Creates a Serilog logger from configuration
        /// </summary>
        public static ILogger CreateLogger(
            IConfiguration configuration,
            string configSection = "Serilog")
        {
            var config = configuration
                .GetValidatedConfiguration<SerilogConfiguration>(configSection);
                
            var loggerConfig = new LoggerConfiguration();
            ConfigureSerilog(loggerConfig, config, configuration);
            
            return loggerConfig.CreateLogger();
        }
        
        /// <summary>
        /// Adds Serilog to services with configuration
        /// </summary>
        public static IServiceCollection AddDomesticatedSerilog(
            this IServiceCollection services,
            IConfiguration configuration,
            string configSection = "Serilog")
        {
            var logger = CreateLogger(configuration, configSection);
            Log.Logger = logger;
            
            services.AddSingleton(logger);
            services.AddLogging(builder => builder.AddSerilog(logger));
            
            return services;
        }
        
        private static void ConfigureSerilog(
            LoggerConfiguration loggerConfiguration,
            SerilogConfiguration config,
            IConfiguration appConfiguration)
        {
            // Set minimum level
            loggerConfiguration.MinimumLevel.Is(config.MinimumLevel);
            
            // Apply overrides
            foreach (var (nameSpace, level) in config.Overrides)
            {
                loggerConfiguration.MinimumLevel.Override(nameSpace, level);
            }
            
            // Add enrichers
            if (config.EnrichWithThreadInfo)
                loggerConfiguration.Enrich.WithThreadId().Enrich.WithThreadName();
                
            if (config.EnrichWithProcessInfo)
                loggerConfiguration.Enrich.WithProcessId().Enrich.WithProcessName();
                
            if (config.EnrichWithEnvironmentInfo)
                loggerConfiguration.Enrich.WithEnvironmentName().Enrich.WithEnvironmentUserName();
                
            if (config.EnrichWithMemoryInfo)
                loggerConfiguration.Enrich.WithMemoryUsage();
            
            // Add custom properties
            foreach (var (key, value) in config.Properties)
            {
                loggerConfiguration.Enrich.WithProperty(key, value);
            }
            
            // Configure console output
            if (config.Console.Enabled)
            {
                var consoleConfig = loggerConfiguration.WriteTo.Console(
                    outputTemplate: config.Console.OutputTemplate,
                    restrictedToMinimumLevel: config.MinimumLevel);
            }
            
            // Configure file output
            if (config.File?.Enabled == true)
            {
                loggerConfiguration.WriteTo.File(
                    path: config.File.Path,
                    rollingInterval: config.File.RollingInterval,
                    fileSizeLimitBytes: config.File.FileSizeLimitBytes,
                    retainedFileCountLimit: config.File.RetainedFileCountLimit,
                    outputTemplate: config.File.OutputTemplate,
                    restrictedToMinimumLevel: config.MinimumLevel);
            }
            
            // Allow additional configuration from appsettings
            loggerConfiguration.ReadFrom.Configuration(appConfiguration);
        }
    }
}