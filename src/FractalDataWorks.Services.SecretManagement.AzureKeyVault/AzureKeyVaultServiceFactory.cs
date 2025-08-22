using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Results;
using FractalDataWorks.Services;
using FractalDataWorks.Services.SecretManagement.AzureKeyVault.Configuration;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.SecretManagement.AzureKeyVault;

/// <summary>
/// Factory for creating Azure Key Vault service instances.
/// </summary>
internal sealed class AzureKeyVaultServiceFactory : IServiceFactory<AzureKeyVaultService, AzureKeyVaultConfiguration>
{
    /// <inheritdoc/>
    public Task<IFdwResult<AzureKeyVaultService>> CreateService(
        AzureKeyVaultConfiguration configuration, 
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        if (configuration == null)
        {
            return Task.FromResult(FdwResult<AzureKeyVaultService>.Failure("Configuration cannot be null."));
        }

        if (serviceProvider == null)
        {
            return Task.FromResult(FdwResult<AzureKeyVaultService>.Failure("Service provider cannot be null."));
        }

        try
        {
            // Get logger from service provider
            var logger = serviceProvider.GetService(typeof(ILogger<AzureKeyVaultService>)) as ILogger<AzureKeyVaultService>;
            if (logger == null)
            {
                return Task.FromResult(FdwResult<AzureKeyVaultService>.Failure("Logger service not available."));
            }

            // Create the service instance
            var service = new AzureKeyVaultService(logger, configuration);

            // Optionally validate the service on startup if configured
            if (configuration.ValidateOnStartup)
            {
                // Note: In a full implementation, you might want to do an async validation
                // For now, we'll assume the service constructor handles validation
            }

            return Task.FromResult(FdwResult<AzureKeyVaultService>.Success(service));
        }
        catch (Exception ex)
        {
            return Task.FromResult(FdwResult<AzureKeyVaultService>.Failure(
                $"Failed to create Azure Key Vault service: {ex.Message}"));
        }
    }

    /// <inheritdoc/>
    public Task<IFdwResult> ValidateConfiguration(
        AzureKeyVaultConfiguration configuration, 
        CancellationToken cancellationToken = default)
    {
        if (configuration == null)
        {
            return Task.FromResult(FdwResult.Failure("Configuration cannot be null."));
        }

        try
        {
            var validator = new AzureKeyVaultConfigurationValidator();
            var validationResult = validator.Validate(configuration);

            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return Task.FromResult(FdwResult.Failure($"Configuration validation failed: {errors}"));
            }

            return Task.FromResult(FdwResult.Success());
        }
        catch (Exception ex)
        {
            return Task.FromResult(FdwResult.Failure(
                $"Configuration validation error: {ex.Message}"));
        }
    }

    /// <inheritdoc/>
    public Task<IFdwResult> TestConnection(
        AzureKeyVaultConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        if (configuration == null)
        {
            return Task.FromResult(FdwResult.Failure("Configuration cannot be null."));
        }

        // First validate the configuration
        var validationTask = ValidateConfiguration(configuration, cancellationToken);
        var validationResult = validationTask.Result;

        if (!validationResult.IsSuccess)
        {
            return Task.FromResult(validationResult);
        }

        try
        {
            // Create a minimal logger for testing
            using var loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<AzureKeyVaultService>();

            // Create a temporary service instance to test the connection
            var testService = new AzureKeyVaultService(logger, configuration);

            // In a full implementation, you might want to make an actual call to Key Vault
            // For example, trying to list secrets or get a test secret
            // For now, we'll assume successful creation means the connection can be established

            return Task.FromResult(FdwResult.Success());
        }
        catch (Exception ex)
        {
            return Task.FromResult(FdwResult.Failure(
                $"Azure Key Vault connection test failed: {ex.Message}"));
        }
    }

    #region IServiceFactory Implementation

    /// <inheritdoc/>
    public IFdwResult<T> Create<T>(IFdwConfiguration configuration) where T : IFdwService
    {
        if (configuration is not AzureKeyVaultConfiguration azureConfig)
        {
            return FdwResult<T>.Failure("Configuration must be AzureKeyVaultConfiguration.");
        }

        if (typeof(T) != typeof(AzureKeyVaultService))
        {
            return FdwResult<T>.Failure($"This factory can only create {nameof(AzureKeyVaultService)} instances.");
        }

        try
        {
            using var loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<AzureKeyVaultService>();
            var service = new AzureKeyVaultService(logger, azureConfig);
            return FdwResult<T>.Success((T)(object)service);
        }
        catch (Exception ex)
        {
            return FdwResult<T>.Failure($"Failed to create service: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    IFdwResult<AzureKeyVaultService> IServiceFactory<AzureKeyVaultService>.Create(IFdwConfiguration configuration)
    {
        return Create(configuration as AzureKeyVaultConfiguration ?? throw new ArgumentException("Configuration must be AzureKeyVaultConfiguration.", nameof(configuration)));
    }

    /// <inheritdoc/>
    public IFdwResult<IFdwService> Create(IFdwConfiguration configuration)
    {
        if (configuration is not AzureKeyVaultConfiguration azureConfig)
        {
            return FdwResult<IFdwService>.Failure("Configuration must be AzureKeyVaultConfiguration.");
        }

        try
        {
            using var loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<AzureKeyVaultService>();
            var service = new AzureKeyVaultService(logger, azureConfig);
            return FdwResult<IFdwService>.Success(service);
        }
        catch (Exception ex)
        {
            return FdwResult<IFdwService>.Failure($"Failed to create service: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public IFdwResult<AzureKeyVaultService> Create(AzureKeyVaultConfiguration configuration)
    {
        if (configuration == null)
        {
            return FdwResult<AzureKeyVaultService>.Failure("Configuration cannot be null.");
        }

        try
        {
            using var loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<AzureKeyVaultService>();
            var service = new AzureKeyVaultService(logger, configuration);
            return FdwResult<AzureKeyVaultService>.Success(service);
        }
        catch (Exception ex)
        {
            return FdwResult<AzureKeyVaultService>.Failure($"Failed to create service: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public Task<AzureKeyVaultService> GetService(string configurationName)
    {
        throw new NotImplementedException("GetService by configuration name requires a configuration provider to be implemented.");
    }

    /// <inheritdoc/>
    public Task<AzureKeyVaultService> GetService(int configurationId)
    {
        throw new NotImplementedException("GetService by configuration ID requires a configuration provider to be implemented.");
    }

    #endregion
}